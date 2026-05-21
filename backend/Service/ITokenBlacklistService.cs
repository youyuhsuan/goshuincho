namespace backend.Services
{
    public interface ITokenBlacklistService
    {
        Task<bool> IsTokenRevokedAsync(string jti);
        Task RevokeTokenAsync(string jti, string userId, DateTime expiresAt, string reason = "logout");
    }
}