using backend.DTOs;
using backend.DTOs.Responses;

namespace backend.Services
{
    public interface IOAuthService
    {
        string GetAuthorizationUrl(string provider);
        Task<(UserDto user, GoogleTokenResponse token)> ExchangeCodeForTokenAsync(string provider, string code, string state);
    }
}