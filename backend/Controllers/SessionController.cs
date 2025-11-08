using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using backend.DTOs;
using backend.Common;
using backend.Services;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SessionsController : ControllerBase
    {
        private readonly ISessionService _sessionService;

        public SessionsController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }


        // GET: api/sessions
        // Get current session information
        [HttpGet()]
        [Authorize]
        public async Task<ActionResult<SessionDto>> GetCurrentSession()
        {
            var sessionId = User.FindFirst("jti")?.Value; // JWT ID claim
            if (string.IsNullOrEmpty(sessionId))
                return Unauthorized();

            var session = await _sessionService.GetSessionByIdAsync(Guid.Parse(sessionId));
            return Ok(session);
        }

        // POST: api/sessions
        // Create new session (Login)
        [HttpPost]
        public async Task<ActionResult<SessionDto>> CreateSession(CreateSessionRequest request)
        {
            var session = await _sessionService.CreateSessionAsync(request);
            return Ok(session);
        }

        // DELETE: api/sessions
        // Delete current session (Logout current session)
        [HttpDelete()]
        [Authorize]
        public async Task<IActionResult> DeleteCurrentSession()
        {
            var sessionId = User.FindFirst("jti")?.Value; // JWT ID claim
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _sessionService.DeleteSessionAsync(Guid.Parse(sessionId), Guid.Parse(userId));
            return NoContent();
        }
    }
}