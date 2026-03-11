public interface IJwtTokenGenerator
{
    string GenerateToken(string sessionId, string userId, string email, string name, DateTime? expiresAt);
}