using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string? GoogleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Picture { get; set; } = string.Empty;
    }

}