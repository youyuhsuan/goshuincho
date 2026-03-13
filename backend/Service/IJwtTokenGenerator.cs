using System.Security.Claims;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(string userId, string email, string name);
    ClaimsPrincipal? ValidateRefreshToken(string refreshToken);
    string GenerateRefreshToken(string userId, DateTime? expiresAt);
}