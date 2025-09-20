using backend.DTOs;
using backend.Models;

namespace backend.Services
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(Guid id);
        Task<Guid> CreateUserAsync(CreateUserRequest request);
        Task UpdateUserAsync(Guid id, UpdateUserRequest request);
        Task DeleteUserAsync(Guid id);
    }
}