using backend.DTOs;
using backend.Models;
using backend.Models.Responses;


namespace backend.Services
{
    public interface IUserService
    {
        Task<UserDto?> GetUserByIdAsync(Guid id);
        Task<Guid> GetOrCreateByGoogleIdAsync(UserInfo googleUser);
        Task CreateUserAsync(CreateUserRequest request);
        Task UpdateUserAsync(Guid id, UpdateUserRequest request);
        Task DeleteUserAsync(Guid id);
    }
}