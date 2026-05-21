using Microsoft.EntityFrameworkCore;

using backend.Data;
using backend.Models;
using backend.Exceptions;
using backend.DTOs;
using backend.DTOs.Requests;
using backend.DTOs.Responses;

using BCrypt.Net;

using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace backend.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public UserService(AppDbContext context, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }

        // Get user by ID
        public async Task<MeDto> GetMeAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {id} not found");
            }

            return new MeDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Picture = user.Picture,
            };
        }

        // Get user by ID
        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {id} not found");
            }

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Picture = user.Picture,
                Bio = user.Bio,
                Location = user.Location,
                FavoriteGoods = user.FavoriteGoods is null
            ? []
            : JsonSerializer.Deserialize<List<string>>(user.FavoriteGoods) ?? [],
                BirthDate = user.BirthDate,
            };
        }

        // Create new user with email/password
        public async Task CreateUserAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new ConflictException("Email already exists");
            }

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        // Get or create user by Google ID
        public async Task<Guid> GetOrCreateByGoogleIdAsync(OAuthUserDto googleUser)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleUser.GoogleId);

            if (user != null) return user.Id;
            user = new User
            {
                Id = Guid.NewGuid(),
                GoogleId = googleUser.GoogleId,
                Email = googleUser.Email,
                Name = googleUser.Name,
                Picture = googleUser.Picture,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user.Id;
        }

        // Update user details (name/email)
        public async Task UpdateUserAsync(Guid id, UpdateUserRequest request)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {id} not found");
            }

            if (request.Name != null) user.Name = request.Name;
            if (request.Bio != null) user.Bio = request.Bio;
            if (request.Location != null) user.Location = request.Location;
            if (request.BirthDate != null) user.BirthDate = request.BirthDate;
            await _context.SaveChangesAsync();
        }

        // Delete user by Id
        public async Task DeleteUserAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new NotFoundException($"User with ID {id} not found");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProfilePictureAsync(Guid id, string pictureUrl)
        {
            var user = await _context.Users.FindAsync(id)
                ?? throw new NotFoundException($"User {id} not found");

            user.Picture = pictureUrl;
            await _context.SaveChangesAsync();
        }

        public async Task ForgotPasswordAsync(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return; // silently return — do not reveal if email exists

            // Remove any existing reset tokens for this user
            var existing = _context.PasswordResetTokens.Where(t => t.UserId == user.Id);
            _context.PasswordResetTokens.RemoveRange(existing);

            // Use URL-safe Base64 (no +, /, or = chars) so the token survives email clients
            // that mangle percent-encoded characters in href attributes.
            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');
            var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

            _context.PasswordResetTokens.Add(new PasswordResetToken
            {
                UserId = user.Id,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            var frontendBaseUrl = _configuration["AppSettings:FrontendBaseUrl"] ?? "http://localhost:5173";
            var resetUrl = $"{frontendBaseUrl}/auth/reset-password?token={rawToken}";
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetUrl);
        }

        public async Task ResetPasswordAsync(string token, string newPassword)
        {
            var tokenHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

            var record = await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

            if (record == null || record.ExpiresAt < DateTime.UtcNow)
                throw new UnprocessableContent("Password reset token is invalid or has expired.");

            record.User!.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.PasswordResetTokens.Remove(record);
            await _context.SaveChangesAsync();
        }

        // Validate user credentials for login
        public async Task<UserDto> ValidateCredentialsAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || string.IsNullOrEmpty(user.Password))
                throw new UnprocessableContent("Invalid email or password");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                throw new UnprocessableContent("Invalid email or password");

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            }
            ;
        }
    }
}