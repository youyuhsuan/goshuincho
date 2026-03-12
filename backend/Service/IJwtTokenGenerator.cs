public interface IJwtTokenGenerator
{
    string GenerateAccessToken(string sessionId, string userId, string email, string name);
    string GenerateRefreshToken(string sessionId, string userId, DateTime? expiresAt);

}