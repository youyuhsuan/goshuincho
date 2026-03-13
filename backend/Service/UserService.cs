using Microsoft.EntityFrameworkCore;

using backend.Data;
using backend.Models;
using backend.DTOs;
using backend.Exceptions;
using backend.Models.Responses;

using BCrypt.Net;


namespace backend.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;
        public UserService(AppDbContext context)
        {
            _context = context;
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
                Email = user.Email
            };
        }

        // Create new user with email/password
        public async Task CreateUserAsync(CreateUserRequest request)
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
        public async Task<Guid> GetOrCreateByGoogleIdAsync(UserInfo googleUser)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.GoogleId == googleUser.Id);

            if (user != null) return user.Id;
            user = new User
            {
                Id = Guid.NewGuid(),
                GoogleId = googleUser.Id,
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


            if (await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != id))
            {
                throw new ConflictException("Email already exists");
            }

            user.Name = request.Name;
            user.Email = request.Email;

            await _context.SaveChangesAsync();
        }

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

        // Validate user credentials for login
        public async Task<UserDto> ValidateCredentialsAsync(CreateSessionRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                throw new UnauthorizedException("Invalid email or password");

            if (string.IsNullOrEmpty(user.Password))
                throw new UnauthorizedException("This account uses Google login, please sign in with Google");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                throw new UnauthorizedException("Invalid email or password");

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