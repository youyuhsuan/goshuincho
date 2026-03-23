namespace backend.Services
{
    public interface IStorageService
    {

        Task<string> UploadProfilePictureAsync(string userId, IFormFile file);
        Task DeleteProfilePictureAsync(string pictureUrl);
    }
}