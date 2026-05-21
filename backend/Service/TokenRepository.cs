using Microsoft.EntityFrameworkCore;
using backend.Data;
using backend.Models;

namespace backend.Services
{
    public class TokenRepository : ITokenRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TokenRepository> _logger;

        public TokenRepository(AppDbContext context, ILogger<TokenRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task CreateAsync(RefreshToken token)
        {
            await _context.RefreshTokens.AddAsync(token);
            await _context.SaveChangesAsync();
            _logger.LogInformation("TokenRepository: Created refresh token for user {UserId}", token.UserId);
        }

        public async Task<RefreshToken?> FindByHashAsync(string tokenHash)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(t =>
                    t.TokenHash == tokenHash &&
                    t.ExpiresAt > DateTime.UtcNow &&
                    t.RevokedAt == null
                );
        }

        public async Task RevokeByUserIdAsync(Guid userId, string reason = "logout")
        {
            var tokens = await _context.RefreshTokens
                .Where(t =>
                    t.UserId == userId &&
                    t.RevokedAt == null &&
                    t.ExpiresAt > DateTime.UtcNow
                )
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.RevokedAt = DateTime.UtcNow;
                token.Reason = reason;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation(
                "TokenRepository: Revoked {Count} token(s) for user {UserId}, reason: {Reason}",
                tokens.Count, userId, reason
            );
        }

        public async Task RevokeAsync(Guid tokenId, string reason = "rotation")
        {
            var token = await _context.RefreshTokens.FindAsync(tokenId);
            if (token == null) return;

            token.RevokedAt = DateTime.UtcNow;
            token.Reason = reason;

            await _context.SaveChangesAsync();
            _logger.LogInformation(
                "TokenRepository: Revoked token {TokenId}, reason: {Reason}",
                tokenId, reason
            );
        }
    }
}