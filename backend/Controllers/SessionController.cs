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
        private readonly ISessionService _sessionService;
        private readonly ICookieService _cookieService;
        private readonly ILogger<SessionsController> _logger;


        public SessionsController(
            IJwtTokenGenerator jwtGenerator,
            ITokenBlacklistService blacklistService,
            ISessionService sessionService,
            ICookieService cookieService,
            ILogger<SessionsController> logger)
        {
            _jwtGenerator = jwtGenerator;
            _blacklistService = blacklistService;
            _sessionService = sessionService;
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
            // Extract session ID from JWT jti claim
            var sessionId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(sessionId))
            {
                _logger.LogWarning("GetCurrentSession: jti claim missing");
                return Unauthorized();
            }

            // Reject request if token has been blacklisted
            var isRevoked = await _blacklistService.IsTokenRevokedAsync(sessionId);
            if (isRevoked)
            {
                _logger.LogWarning("GetCurrentSession: Token {SessionId} has been revoked", sessionId);
                return Unauthorized();
            }

            var session = await _sessionService.GetSessionByIdAsync(Guid.Parse(sessionId));
            return Ok(session);
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
            // Validate credentials and create session record in database
            var session = await _sessionService.CreateSessionAsync(request);

            // Generate JWT token using session ID as jti claim
            var accessToken = _jwtGenerator.GenerateAccessToken(
                sessionId: session.Id.ToString(),
                userId: session.UserId.ToString(),
                email: request.Email,
                name: session.Name
            );

            var refreshToken = _jwtGenerator.GenerateRefreshToken(
               sessionId: session.Id.ToString(),
               userId: session.UserId.ToString(),
               expiresAt: session.ExpiresAt
           );

            // Store token in HttpOnly cookie to prevent XSS attacks
            _cookieService.SetAuthCookies(Response, accessToken, refreshToken, session.ExpiresAt);
            _logger.LogInformation("CreateSession: User {UserId} logged in", session.UserId);

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
            var sessionId = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var expiresClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;


            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(expiresClaim))
            {
                _logger.LogWarning("DeleteCurrentSession: Missing claims");
                return Unauthorized();
            }

            // Blacklist the token first to prevent any in-flight requests from passing
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expiresClaim!)).UtcDateTime;
            await _blacklistService.RevokeTokenAsync(sessionId, userId, expiresAt);

            // Remove session record from database
            await _sessionService.DeleteSessionAsync(Guid.Parse(sessionId), Guid.Parse(userId));

            // Clear authentication cookies
            _cookieService.ClearAuthCookies(Response);
            _logger.LogInformation("DeleteCurrentSession: User {UserId} logged out", userId);

            return NoContent();
        }
    }
}