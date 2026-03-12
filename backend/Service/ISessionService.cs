using backend.DTOs;

namespace backend.Services
{
    public interface ISessionService
    {
        Task<SessionSummaryDto> GetSessionByIdAsync(Guid sessionId);
        Task<Guid> CreateOAuthSessionAsync(Guid userId);
        Task<SessionDto> CreateSessionAsync(CreateSessionRequest request);
        Task DeleteSessionAsync(Guid sessionId, Guid userId);
    }
}
