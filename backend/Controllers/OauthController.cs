using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.Models.Requests;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/oauth")]
    public class OAuthController : ControllerBase
    {


        private readonly IJwtTokenGenerator _jwtGenerator;
        private readonly ICookieService _cookieService;
        private readonly ISessionService _sessionService;
        private readonly IOAuthService _oauthService;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<OAuthController> _logger;

        public OAuthController(
            IJwtTokenGenerator jwtGenerator,
            ICookieService cookieService,
            IOAuthService oauthService,
            ISessionService sessionService,
            IWebHostEnvironment environment,
            ILogger<OAuthController> logger)
        {
            _jwtGenerator = jwtGenerator;
            _cookieService = cookieService;
            _oauthService = oauthService;
            _sessionService = sessionService;
            _environment = environment;
            _logger = logger;
        }

        /// POST: api/oauth/authorizations
        /// <summary>
        /// Creates an OAuth authorization request and returns the authorization URL
        /// </summary>
        /// <response code="200">Returns the authorization URL</response>
        /// <response code="400">
        /// Validation failed
        /// {
        ///   "Provider": ["Provider is required"],
        /// }
        /// </response>
        [HttpPost("authorizations")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreateAuthorization([FromBody] AuthorizationRequest request)
        {
            var authUrl = _oauthService.GetAuthorizationUrl(request.Provider);
            return Ok(authUrl);
        }

        /// POST: api/oauth/tokens
        /// <summary>
        /// Exchange an authorization code for access token
        /// </summary>
        /// <response code="200">Authentication successful, tokens set in HttpOnly cookies</response>
        /// <response code="400">
        /// Validation failed
        /// {
        ///   "Provider": ["Provider is required"],
        ///   "Code": ["Code is required"],
        ///   "State": ["State is required"]
        /// }
        /// </response>
        /// <response code="422">Invalid or expired state parameter</response>
        [HttpPost("tokens")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExchangeToken([FromBody] TokenRequest request)
        {
            var (userInfo, token) = await _oauthService.ExchangeCodeForTokenAsync(
                request.Provider, request.Code, request.State);
            _logger.LogInformation("userInfo: {@UserInfo}", userInfo);
            var session = await _sessionService.CreateOAuthSessionAsync(userInfo.Id);

            var jwtToken = _jwtGenerator.GenerateToken(
                sessionId: session.ToString(),
                userId: userInfo.Id,
                email: userInfo.Email,
                name: userInfo.Name,
                expiresAt: DateTime.UtcNow.AddHours(1)
                );
            _cookieService.SetAuthCookies(Response, jwtToken, token.refresh_token, token.expires_in);

            // Return token in response body as per RFC 6749 Section 5.1.
            // The OAuth 2.0 specification requires token endpoint to respond with 
            // HTTP 200 OK and a JSON body containing access_token and token_type,
            // even when using cookies for token delivery.
            return Ok();
        }
    }
}