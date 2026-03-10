using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;
using backend.DTOs;
using backend.Exceptions;
using BCrypt.Net;

namespace backend.Services
{
    public class SessionService : ISessionService
    {
        private readonly AppDbContext _context;

        public SessionService(AppDbContext context)
        {
            _context = context;
        }

        // Retrieves an active session by its ID.
        public async Task<SessionSummaryDto> GetSessionByIdAsync(Guid sessionId)
        {
            var session = await _context.Sessions
                .Where(s => s.Id == sessionId && s.IsActive)
                .Select(s => new SessionSummaryDto
                {
                    Id = s.Id,
                    ExpiresAt = s.ExpiresAt,
                    IsActive = s.IsActive
                })
                .FirstOrDefaultAsync();
            if (session == null)
            {
                throw new NotFoundException($"Session with ID {sessionId} not found");
            }

            return session;
        }

        // Creates a new session for a user after validating credentials.
        public async Task<SessionDto> CreateSessionAsync(CreateSessionRequest request)
        {
            // Verify user exists and password is correct
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !VerifyPassword(request.Password, user.Password))
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            // Revoke all existing active sessions for this user
            await RevokeExistingSessionsAsync(user.Id);

            // Set expiry based on RememberMe flag
            var expiry = request.RememberMe
                ? DateTime.UtcNow.AddDays(30)
                : DateTime.UtcNow.AddHours(1);

            var session = await CreateNewSessionAsync(user.Id, expiry);

            return new SessionDto
            {
                Id = session.Id,
                UserId = session.UserId,
                Name = user.Name,
                ExpiresAt = session.ExpiresAt,
                CreatedAt = session.CreatedAt,
                IsActive = session.IsActive
            };
        }

        // Creates a new session for a user authenticated via OAuth.
        public async Task<Guid> CreateOAuthSessionAsync(string userId)
        {
            var userGuid = Guid.Parse(userId);

            // Revoke all existing active sessions for this user
            await RevokeExistingSessionsAsync(userGuid);

            var session = await CreateNewSessionAsync(userGuid, DateTime.UtcNow.AddHours(1));
            return session.Id;
        }

        // Revokes the current session on logout.
        public async Task DeleteSessionAsync(Guid sessionId, Guid userId)
        {
            var session = await _context.Sessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

            if (session == null)
                throw new NotFoundException("Session not found");

            if (!session.IsActive)
                throw new ConflictException("Session already revoked");

            // Mark session as revoked
            session.IsActive = false;
            session.RevokedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // Creates and persists a new session entity with the given user ID and expiry.
        private async Task<Session> CreateNewSessionAsync(Guid userId, DateTime expiry)
        {
            var session = new Session
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ExpiresAt = expiry,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
            };

            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }


        // Revokes all active sessions for a given user.
        private async Task RevokeExistingSessionsAsync(Guid userId)
        {
            var existingSessions = await _context.Sessions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync();

            foreach (var existingSession in existingSessions)
            {
                existingSession.IsActive = false;
                existingSession.RevokedAt = DateTime.UtcNow;
            }
        }

        // Verifies a plain text password against a BCrypt hashed password.
        private bool VerifyPassword(string inputPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, hashedPassword);
        }
    }
}