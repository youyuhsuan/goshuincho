using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

using backend.DTOs;
using backend.DTOs.Requests;
using backend.DTOs.Responses;
using backend.Models;
using backend.Services;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtTokenGenerator _jwtGenerator;
        private readonly ITokenBlacklistService _blacklistService;
        private readonly ITokenRepository _tokenRepository;
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;


        public AuthController(
            IJwtTokenGenerator jwtGenerator,
            ITokenBlacklistService blacklistService,
            ITokenRepository tokenRepository,
            IUserService userService,
            ILogger<AuthController> logger)
        {
            _jwtGenerator = jwtGenerator;
            _tokenRepository = tokenRepository;
            _userService = userService;
            _blacklistService = blacklistService;
            _logger = logger;
        }

        // GET: api/auth/me
        /// <summary>
        /// Get current authenticated user info
        /// </summary>
        /// <response code="200">User info returned successfully</response>
        /// <response code="401">Token is missing or has been revoked</response>
        /// <response code="404">User not found</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<MeDto>> GetCurrentAuth()
        {
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("GetCurrentAuth: Missing claims");
                return Unauthorized();
            }

            // Reject request if token has been blacklisted
            if (await _blacklistService.IsTokenRevokedAsync(jti))
            {
                _logger.LogWarning("GetCurrentAuth: Token {UserId} has been revoked", userId);
                return Unauthorized();
            }

            var user = await _userService.GetMeAsync(Guid.Parse(userId));
            return Ok(user);
        }

        /// POST: api/auth/register
        /// <summary>
        /// Register a new user account
        /// </summary>
        /// <response code="201">User account created sucessfully</response>
        /// <response code="400">Validation failed</response>
        /// <response code="422">Email already in use</response>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult> Register(RegisterRequest request)
        {
            await _userService.CreateUserAsync(request);
            return Created();
        }

        /// POST: api/auth/login
        /// <summary>
        /// Authenticate user and issue tokens
        /// </summary>
        /// <response code="200">Tokens issued sucessfully</response>
        /// <response code="400">Validation failed</response>
        /// <response code="422">Invalid email or password</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<AuthDto>> Login(LoginRequest request)
        {
            var user = await _userService.ValidateCredentialsAsync(request);

            // Generate JWT token using user ID as jti claim
            var accessToken = _jwtGenerator.GenerateAccessToken(
                userId: user.Id.ToString(),
                email: user.Email,
                name: user.Name
            );

            // Set refresh token expiry based on "Remember Me" option
            var refreshExpiry = request.RememberMe
                ? DateTime.UtcNow.AddDays(30)
                : DateTime.UtcNow.AddDays(7);

            // Generate refresh token with same user ID for easy revocation
            var newRawToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
            var newHash = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(newRawToken)
                )
            );
            await _tokenRepository.CreateAsync(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = newHash,
                ExpiresAt = refreshExpiry
            });

            _logger.LogInformation("Login: User {UserId} logged in", user.Id);

            return Ok(new AuthDto
            {
                AccessToken = accessToken,
                RefreshToken = newRawToken
            });
        }

        /// POST: api/auth/logout
        /// <summary>
        /// Logout current user
        /// </summary>
        /// <response code="204">Logged out successfully</response>
        /// <response code="401">Token is missing or has been revoked</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> DeleteCurrentAuth()
        {
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var expiresClaim = User.FindFirst("exp")?.Value;

            if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(expiresClaim))
            {
                _logger.LogWarning("DeleteCurrentAuth: Missing claims");
                return Unauthorized();
            }

            //  Add access token to blacklist to prevent user after logout
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiresClaim)).UtcDateTime;
            await _blacklistService.RevokeTokenAsync(jti, userId, expiresAt);

            // Soft delete refresh token from DB to prevent issuing access token
            await _tokenRepository.RevokeByUserIdAsync(Guid.Parse(userId), "logout");


            _logger.LogInformation("DeleteCurrentAuth: User {UserId} logged out", userId);

            return NoContent();
        }

        /// POST: api/auth/forgot-password
        /// <summary>
        /// Request a password reset email
        /// </summary>
        /// <response code="200">Request processed (always returns 200 to avoid email enumeration)</response>
        /// <response code="400">Validation failed</response>
        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            await _userService.ForgotPasswordAsync(request.Email);
            return Ok();
        }

        /// POST: api/auth/reset-password
        /// <summary>
        /// Reset password using a valid token
        /// </summary>
        /// <response code="204">Password reset successfully</response>
        /// <response code="400">Validation failed</response>
        /// <response code="422">Token is invalid or expired</response>
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            await _userService.ResetPasswordAsync(request.Token, request.NewPassword);
            return NoContent();
        }

        /// POST: api/auth/refresh
        /// <summary>
        /// Issue a new Access Token using a valid Refresh Token
        /// </summary>
        /// <response code="200">New Access Token issued successfully</response>
        /// <response code="401">Refresh Token is invalid, expired, or has been revoked</response>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public async Task<ActionResult<AuthDto>> RefreshToken(RefreshTokenRequest request)
        {
            // Hash the incoming raw token to look up in DB
            var hash = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(request.RefreshToken)
                )
            );

            // Find token in DB — rejects if expired or already revoked
            var stored = await _tokenRepository.FindByHashAsync(hash);
            if (stored == null)
            {
                _logger.LogWarning("RefreshToken: Token not found, expired, or revoked");
                return Unauthorized();
            }

            // Fetch the associated user
            var user = await _userService.GetMeAsync(stored.UserId);

            // Issue new Access Token
            var newAccessToken = _jwtGenerator.GenerateAccessToken(
                userId: user.Id.ToString(),
                email: user.Email,
                name: user.Name
            );

            // Refresh Token Rotation — revoke old token and issue a new one
            await _tokenRepository.RevokeAsync(stored.Id, "rotation");

            var newRawToken = Convert.ToBase64String(
                System.Security.Cryptography.RandomNumberGenerator.GetBytes(64)
            );
            var newHash = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(newRawToken)
                )
            );
            await _tokenRepository.CreateAsync(new RefreshToken
            {
                UserId = stored.UserId,
                TokenHash = newHash,
                ExpiresAt = stored.ExpiresAt  // Preserve original expiry
            });

            _logger.LogInformation("RefreshToken: User {UserId} token rotated", stored.UserId);

            return Ok(new AuthDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRawToken
            });
        }
    }
}