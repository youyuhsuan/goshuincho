using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.Models.Requests;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/oauth")]
    public class OAuthController : ControllerBase
    {
        private readonly IOAuthService _oauthService;
        private readonly IWebHostEnvironment _environment;

        public OAuthController(
            IOAuthService oauthService,
            IWebHostEnvironment environment,
            ILogger<OAuthController> logger)
        {
            _oauthService = oauthService;
            _environment = environment;
        }

        // POST: api/oauth/authorizations
        // Creates an OAuth authorization request and returns the authorization URL
        [HttpPost("authorizations")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CreateAuthorization([FromBody] AuthorizationRequest request)
        {
            var authUrl = _oauthService.GetAuthorizationUrl(request.Provider);
            return Ok(authUrl);
        }

        // POST: api/oauth/tokens
        // Exchange an authorization code for access token
        [HttpPost("tokens")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ExchangeToken([FromBody] TokenRequest request)
        {
            var (user, token) = await _oauthService.ExchangeCodeForTokenAsync(request.Provider, request.Code, request.State);
            SetAuthCookies(token.access_token, token.refresh_token, token.expires_in);
            Console.WriteLine("ExchangeToken AFTER token exchange");

            return Ok(user);
        }

        private void SetAuthCookies(string accessToken, string? refreshToken, int expiresIn)
        {
            // Set the access token cookie
            Response.Cookies.Append("access_token", accessToken, new CookieOptions
            {
                HttpOnly = true,                     // Cookie cannot be accessed via JavaScript (protects against XSS)
                Secure = true,                       // Cookie will only be sent over HTTPS
                SameSite = SameSiteMode.None,        // Cookie can be sent in cross-site requests (needed for cross-origin requests)
                MaxAge = TimeSpan.FromSeconds(expiresIn), // Cookie expiration time in seconds
                Path = "/",                          // Cookie is available to the entire site
            });

            if (!string.IsNullOrEmpty(refreshToken))
            {
                // Set the refresh token cookie
                Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    MaxAge = TimeSpan.FromDays(30),
                    Path = "/"
                });
            }
        }
    }
}