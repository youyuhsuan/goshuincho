using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Requests
{
    public class UpdateUserRequest
    {
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}