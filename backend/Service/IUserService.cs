using backend.Models;
using backend.DTOs;
using backend.DTOs.Requests;
using backend.DTOs.Responses;

namespace backend.Services
{
    public interface IUserService
    {
        Task<MeDto> GetUserByIdAsync(Guid id);
        Task CreateUserAsync(RegisterRequest request);
        Task<Guid> GetOrCreateByGoogleIdAsync(UserDto googleUser);
        Task UpdateUserAsync(Guid id, UpdateUserRequest request);
        Task DeleteUserAsync(Guid id);
        Task UpdateProfilePictureAsync(Guid id, string pictureUrl);
        Task<UserDto> ValidateCredentialsAsync(LoginRequest request);
    }
}