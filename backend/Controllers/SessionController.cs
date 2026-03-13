using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

using backend.DTOs;
using backend.Services;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionsController : ControllerBase
    {
        private readonly IJwtTokenGenerator _jwtGenerator;
        private readonly ITokenBlacklistService _blacklistService;
        private readonly IUserService _userService;
        private readonly ICookieService _cookieService;
        private readonly ILogger<SessionsController> _logger;


        public SessionsController(
            IJwtTokenGenerator jwtGenerator,
            ITokenBlacklistService blacklistService,
            IUserService userService,
            ICookieService cookieService,
            ILogger<SessionsController> logger)
        {
            _jwtGenerator = jwtGenerator;
            _userService = userService;
            _blacklistService = blacklistService;
            _cookieService = cookieService;
            _logger = logger;
        }

        // GET: api/sessions
        /// <summary>
        /// Get current session information
        /// </summary>
        /// <response code="200">Returns current session details</response>
        /// <response code="401">Token is missing or has been revoked</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet()]
        [Authorize]
        public async Task<ActionResult<SessionSummaryDto>> GetCurrentSession()
        {
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("GetCurrentSession: Missing claims");
                return Unauthorized();
            }


            // Reject request if token has been blacklisted
            if (await _blacklistService.IsTokenRevokedAsync(jti))
            {
                _logger.LogWarning("GetCurrentSession: Token {UserId} has been revoked", userId);
                return Unauthorized();
            }

            var user = await _userService.GetUserByIdAsync(Guid.Parse(userId));
            return Ok(user);
        }

        /// POST: api/sessions
        /// <summary>
        /// Create new session (Login)
        /// </summary>
        /// <response code="201">Session created successfully</response>
        /// <response code="400">Validation failed</response>
        /// <response code="422">Invalid email or password</response>        
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status422UnprocessableEntity)]
        [HttpPost]
        public async Task<ActionResult> CreateSession(CreateSessionRequest request)
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
            var refreshToken = _jwtGenerator.GenerateRefreshToken(
                userId: user.Id.ToString(),
                expiresAt: refreshExpiry
           );

            // Store token in HttpOnly cookie to prevent XSS attacks
            _cookieService.SetAuthCookies(Response, accessToken, refreshToken, refreshExpiry);
            _logger.LogInformation("CreateSession: User {UserId} logged in", user.Id);

            return Created();
        }

        /// DELETE: api/sessions
        /// <summary>
        /// Delete current session (Logout)
        /// </summary>
        /// <response code="204">Session deleted successfully</response>
        /// <response code="401">Token is missing or has been revoked</response>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [HttpDelete()]
        [Authorize]
        public async Task<IActionResult> DeleteCurrentSession()
        {
            // Extract required claims from JWT token
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var expiresClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;

            if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(expiresClaim))
            {
                _logger.LogWarning("DeleteCurrentSession: Missing claims");
                return Unauthorized();
            }

            // Blacklist the token first to prevent any in-flight requests from passing
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiresClaim!)).UtcDateTime;
            await _blacklistService.RevokeTokenAsync(jti, userId, expiresAt);

            // Clear authentication cookies
            _cookieService.ClearAuthCookies(Response);
            _logger.LogInformation("DeleteCurrentSession: User logged out, token {Jti} revoked", jti);

            return NoContent();
        }


        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshSession()
        {
            var refreshToken = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(refreshToken))
                return Unauthorized();


            var principal = _jwtGenerator.ValidateRefreshToken(refreshToken);
            if (principal == null)
                return Unauthorized();

            var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(jti) || string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("RefreshSession: Missing claims in refresh token");
                return Unauthorized();
            }

            if (await _blacklistService.IsTokenRevokedAsync(jti))
                return Unauthorized();

            var user = await _userService.GetUserByIdAsync(Guid.Parse(userId));
            var newAccessToken = _jwtGenerator.GenerateAccessToken(
                userId: userId,
                email: user.Email,
                name: user.Name
            );

            _logger.LogInformation("RefreshSession: newAccessToken={Token}", newAccessToken);

            // Store token in HttpOnly cookie to prevent XSS attacks
            _cookieService.SetAuthCookies(Response, newAccessToken);
            return Ok();
        }
    }
}