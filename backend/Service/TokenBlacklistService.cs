using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Services
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly AppDbContext _context;

        private readonly ILogger<TokenBlacklistService> _logger;

        public TokenBlacklistService(
            AppDbContext context,
            ILogger<TokenBlacklistService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Checks whether a token has been revoked and has not yet expired.
        public async Task<bool> IsTokenRevokedAsync(string jti)
        {
            // Check if the token exists in the blacklist and has not yet expired
            var isRevoked = await _context.RevokedTokens
                .AnyAsync(rt => rt.Jti == jti && rt.ExpiresAt > DateTime.UtcNow);

            return isRevoked;
        }

        // Adds a token to the blacklist, preventing it from being used in future requests.
        // <param name="jti">The JWT ID (jti) claim uniquely identifying the token.</param>
        // <param name="userId">The ID of the user who owns the token.</param>
        // <param name="expiresAt">The original expiration time of the token.</param>
        // <param name="reason">The reason for revocation (e.g. "logout", "password_change"). Defaults to "logout".</param>
        public async Task RevokeTokenAsync(string jti, string userId, DateTime expiresAt, string reason = "logout")
        {
            // Check if the token is already in the blacklist
            var existing = await _context.RevokedTokens
                .FirstOrDefaultAsync(rt => rt.Jti == jti);

            if (existing != null)
            {
                _logger.LogInformation("Token {Jti} already revoked", jti);
                return;
            }

            // Build the revoked token record and persist it to the database
            var revokedToken = new RevokedToken
            {
                Id = Guid.NewGuid(),
                Jti = jti,
                UserId = userId,
                RevokedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,  // Retain the original expiry so the blacklist entry can be cleaned up later
                Reason = reason
            };

            _context.RevokedTokens.Add(revokedToken);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Token {Jti} revoked for user {UserId}, reason: {Reason}",
                jti, userId, reason);
        }
    }
}