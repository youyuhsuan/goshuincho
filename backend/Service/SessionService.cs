using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;
using backend.Exceptions;

namespace backend.Services
{
    public class SessionService : ISessionService
    {
        private readonly AppDbContext _context;

        public SessionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SessionDto?> GetSessionByIdAsync(Guid sessionId)
        {
            var session = await _context.Sessions
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.IsActive);
            if (session == null)
            {
                throw new NotFoundException($"Session with ID {sessionId} not found");
            }

            return MapToDto(session);
        }

        public async Task<SessionDto?> CreateSessionAsync(CreateSessionRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !VerifyPassword(request.Password, user.Password))
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var existingSessions = await _context.Sessions
                .Where(s => s.UserId == user.Id && s.IsActive)
                .ToListAsync();

            if (existingSessions.Any())
            {
                foreach (var existingSession in existingSessions)
                {
                    existingSession.IsActive = false;
                    existingSession.RevokedAt = DateTime.UtcNow;
                }
            }
            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow,
                IsActive = true,
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return MapToDto(session);
        }

        public async Task DeleteSessionAsync(Guid sessionId, Guid userId)
        {
            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId && s.IsActive);
            if (session == null)
            {
                throw new Exception("Session not found or inactive");
            }
            session.IsActive = false;

            await _context.SaveChangesAsync();
        }

        private SessionDto MapToDto(Session session)
        {
            return new SessionDto
            {
                Id = session.Id,
                UserId = session.UserId,
                AccessToken = session.AccessTokenHash,
                RefreshToken = session.RefreshTokenHash,
                CreatedAt = session.CreatedAt,
                IsActive = session.IsActive
            };
        }

        private bool VerifyPassword(string inputPassword, string hashedPassword)
        {
            return inputPassword == hashedPassword;
            // return BCrypt.Net.BCrypt.Verify(inputPassword, hashedPassword);
        }
    }
}