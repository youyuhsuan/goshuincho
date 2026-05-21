using System.Security.Claims;

public interface IJwtTokenGenerator
{
    string GenerateAccessToken(string userId, string email, string name);
    ClaimsPrincipal? ValidateRefreshToken(string refreshToken);
}