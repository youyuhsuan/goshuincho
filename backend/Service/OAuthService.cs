using System.Security.Cryptography;
using System.Net.Http.Json;
using backend.Services;
using backend.Models.Entities;
using backend.DTOs;
using backend.DTOs.Responses;

namespace backend.Services
{
    public class OAuthService : IOAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, StateData> _stateStore;

        public OAuthService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _stateStore = new Dictionary<string, StateData>();
        }

        // Generates the authorization URL for a given provider (e.g., Google)
        public string GetAuthorizationUrl(string provider)
        {
            var state = GenerateRandomState();
            StoreState(state);
            CleanupExpiredStates();

            switch (provider)
            {
                case "google":
                    return BuildGoogleAuthUrl(state);
                default:
                    throw new NotSupportedException($"Provider '{provider}' is not supported");
            }
        }

        // Exchanges an authorization code for tokens and fetches user info
        public async Task<(UserDto user, GoogleTokenResponse token)> ExchangeCodeForTokenAsync(string provider, string code, string state)
        {
            if (!ValidateAndConsumeState(state))
            {
                throw new InvalidOperationException("Invalid or expired state parameter");
            }

            switch (provider)
            {
                case "google":
                    var tokenResponse = await ExchangeCodeWithGoogleAsync(code);
                    var googleUserInfo = await GetGoogleUserInfoAsync(tokenResponse.access_token);


                    if (!googleUserInfo.verified_email)
                    {
                        throw new InvalidOperationException("Email not verified");
                    }


                    var userInfo = new UserDto
                    {
                        GoogleId = googleUserInfo.id,
                        Email = googleUserInfo.email,
                        Name = googleUserInfo.name,
                        Picture = googleUserInfo.picture
                    };

                    return (userInfo, tokenResponse);
                default:
                    throw new NotSupportedException($"Provider '{provider}' is not supported");
            }

        }

        // Generates a secure random state string for CSRF protection
        private string GenerateRandomState()
        {
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToHexString(bytes).ToLower();
        }

        // Stores the generated state in memory with a timestamp and expiration
        private void StoreState(string state)
        {
            _stateStore[state] = new StateData
            {
                Timestamp = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };
        }

        private bool ValidateAndConsumeState(string state)
        {
            if (!_stateStore.TryGetValue(state, out var stateData))
            {
                return false;
            }

            if (stateData.ExpiresAt < DateTime.UtcNow)
            {
                _stateStore.Remove(state);
                return false;
            }

            _stateStore.Remove(state);
            return true;
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

        private async Task<GoogleTokenResponse> ExchangeCodeWithGoogleAsync(string code)
        {
            var clientId = _configuration["Google:ClientId"];
            var clientSecret = _configuration["Google:ClientSecret"];
            var redirectUri = _configuration["Google:RedirectUri"];

            var oauthRequest = new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", clientId! },
                { "client_secret", clientSecret! },
                { "redirect_uri", redirectUri! },
                { "grant_type", "authorization_code" }
            };

            var response = await _httpClient.PostAsync(
                "https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(oauthRequest)
            );

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<GoogleTokenResponse>()
                ?? throw new Exception("Failed to deserialize token response");
        }

        private async Task<GoogleUserInfo> GetGoogleUserInfoAsync(string accessToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.googleapis.com/oauth2/v2/userinfo");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<GoogleUserInfo>()
                ?? throw new Exception("Failed to deserialize user info");
        }
    }
}