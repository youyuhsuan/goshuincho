using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Picture { get; set; } = string.Empty;
        public string? Bio { get; set; } = string.Empty;
        public string? Location { get; set; } = string.Empty;
        public List<string>? FavoriteGoods { get; set; } = new();
        public DateTime? BirthDate { get; set; }
    }

    public class OAuthUserDto
    {
        public Guid Id { get; set; }
        public string? GoogleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Picture { get; set; } = string.Empty;
    }


    public class MeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Picture { get; set; } = string.Empty;
    }

}