using backend.Models.Responses;

namespace backend.Services
{
    public interface IOAuthService
    {
        string GetAuthorizationUrl(string provider);
        Task<(UserInfo user, GoogleTokenResponse token)> ExchangeCodeForTokenAsync(string provider, string code, string state);
    }
}