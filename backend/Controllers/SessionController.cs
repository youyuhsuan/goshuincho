using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using backend.DTOs;
using backend.Common;
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
        private readonly ILogger<SessionsController> _logger;


        public SessionsController(
            IJwtTokenGenerator jwtGenerator,
            ITokenBlacklistService blacklistService,
            ISessionService sessionService,
             ILogger<SessionsController> logger)
        {
            _jwtGenerator = jwtGenerator;
            _blacklistService = blacklistService;
            _sessionService = sessionService;
            _logger = logger;
        }

        // GET: api/sessions
        // Get current session information
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

        // POST: api/sessions
        // Create new session (Login)
        [HttpPost]
        public async Task<ActionResult> CreateSession(CreateSessionRequest request)
        {
            // Validate credentials and create session record in database
            var session = await _sessionService.CreateSessionAsync(request);

            // Generate JWT token using session ID as jti claim
            var jwtToken = _jwtGenerator.GenerateToken(
                sessionId: session.Id.ToString(),
                userId: session.UserId.ToString(),
                email: request.Email,
                name: session.Name
            );

            // Store token in HttpOnly cookie to prevent XSS attacks
            Response.Cookies.Append("access_token", jwtToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = session.ExpiresAt
            });
            _logger.LogInformation("CreateSession: User {UserId} logged in", session.UserId);

            return NoContent();
        }

        // DELETE: api/sessions
        // Delete current session (Logout current session)
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
            Response.Cookies.Delete("access_token");

            _logger.LogInformation("DeleteCurrentSession: User {UserId} logged out", userId);
            return NoContent();
        }
    }
}