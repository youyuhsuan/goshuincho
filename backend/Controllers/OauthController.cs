using Microsoft.AspNetCore.Mvc;
using backend.Services;
using backend.DTOs;
using backend.DTOs.Requests;
using backend.Models;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/oauth")]
    public class OAuthController : ControllerBase
    {
        private readonly IJwtTokenGenerator _jwtGenerator;
        private readonly IUserService _userService;
        private readonly IOAuthService _oauthService;
        private readonly ITokenRepository _tokenRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<OAuthController> _logger;

        public OAuthController(
            IJwtTokenGenerator jwtGenerator,
            IUserService userService,
            IOAuthService oauthService,
            ITokenRepository tokenRepository,
            IWebHostEnvironment environment,
            ILogger<OAuthController> logger)
        {
            _jwtGenerator = jwtGenerator;
            _tokenRepository = tokenRepository;
            _userService = userService;
            _oauthService = oauthService;
            _environment = environment;
            _logger = logger;
        }

        /// POST: api/oauth/authorizations
        /// <summary>
        /// Creates an OAuth authorization request.
        /// </summary>
        /// <response code="200">Returns the authorization URL</response>
        /// <response code="400">Validation failed</response>
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [HttpPost("authorizations")]
        public IActionResult CreateAuthorization([FromBody] AuthorizationRequest request)
        {
            var authUrl = _oauthService.GetAuthorizationUrl(request.Provider);
            return Ok(authUrl);
        }

        /// POST: api/oauth/tokens
        /// <summary>
        /// Exchange an authorization code for access token.
        /// </summary>
        /// <response code="200"></response>
        /// <response code="400">Validation failed</response>
        /// <response code="422">Invalid or expired state parameter</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("tokens")]
        public async Task<ActionResult<AuthDto>> ExchangeToken([FromBody] OAuthRequest request)
        {
            var (userInfo, token) = await _oauthService.ExchangeCodeForTokenAsync(
                request.Provider, request.Code, request.State);
            _logger.LogInformation("userInfo: {@UserInfo}", userInfo);

            var userId = await _userService.GetOrCreateByGoogleIdAsync(userInfo);

            // Generate JWT tokens
            var accessToken = _jwtGenerator.GenerateAccessToken(
                    userId: userId.ToString(),
                    email: userInfo.Email,
                    name: userInfo.Name
                );

            var newRawToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
            var newHash = Convert.ToBase64String(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(newRawToken)
                )
            );

            await _tokenRepository.CreateAsync(new RefreshToken
            {
                UserId = userId,
                TokenHash = newHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });

            _logger.LogInformation("Login: User {UserId} logged in", userId);


            // Return token in response body as per RFC 6749 Section 5.1.
            // The OAuth 2.0 specification requires token endpoint to respond with 
            // HTTP 200 OK and a JSON body containing access_token and token_type,
            // even when using cookies for token delivery.
            return Ok(new AuthDto
            {
                AccessToken = accessToken,
                RefreshToken = newRawToken
            });
        }
    }
}