using backend.DTOs;

namespace backend.Services
{
    public interface ISessionService
    {
        Task<SessionDto?> GetSessionByIdAsync(Guid sessionId);
        Task<SessionDto?> CreateSessionAsync(CreateSessionRequest request);
        Task DeleteSessionAsync(Guid sessionId, Guid userId);
    }
}
