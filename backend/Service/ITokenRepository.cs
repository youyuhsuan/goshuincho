using backend.Models;

namespace backend.Services
{
    public interface ITokenRepository
    {
        Task CreateAsync(RefreshToken token);
        Task<RefreshToken?> FindByHashAsync(string tokenHash);
        Task RevokeByUserIdAsync(Guid userId, string reason = "logout");
        Task RevokeAsync(Guid tokenId, string reason = "rotation");
    }
}