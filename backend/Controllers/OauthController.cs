using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

public class AuthorizationRequest
{
    public required string Provider { get; set; }
    public string[]? Scopes { get; set; }
    public string? Prompt { get; set; }
    public string? LoginHint { get; set; }
}

namespace backend.Controllers
{
    [ApiController]
    [Route("api/oauth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private static Dictionary<string, StateData> _stateStore = new();

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("authorizations")]
        public IActionResult CreateAuthorization([FromBody] AuthorizationRequest request)
        {
            var state = GenerateRandomState();
            _stateStore[state] = new StateData
            {
                Timestamp = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };

            CleanupExpiredStates();

            var authUrl = "";
            switch (request.Provider)
            {
                case "google":
                    authUrl = BuildGoogleAuthUrl(state);
                    return Ok(authUrl);
                default:
                    return BadRequest(new { error = "Unsupported provider" });
            }

        }



        private string GenerateRandomState()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToHexString(bytes).ToLower();
        }

        private string BuildGoogleAuthUrl(string state)
        {
            var clientId = _configuration["Google:ClientId"];
            var redirectUri = _configuration["Google:RedirectUri"];

            var queryParams = new Dictionary<string, string>
            {
                { "client_id", clientId! },
                { "response_type", "code" },
                { "state", state },
                { "scope", "openid email profile" },
                { "redirect_uri", redirectUri! }
            };

            var queryString = string.Join("&",
                queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));

            return $"https://accounts.google.com/o/oauth2/v2/auth?{queryString}";
        }

        private void CleanupExpiredStates()
        {
            var now = DateTime.UtcNow;
            var expiredKeys = _stateStore
                .Where(kvp => kvp.Value.ExpiresAt < now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _stateStore.Remove(key);
            }
        }
    }

    public class StateData
    {
        public DateTime Timestamp { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}