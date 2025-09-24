using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class SessionDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUsedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateSessionRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(3)]
        [MaxLength(8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, and one number.")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }

    public class UpdateSessionRequest
    {
        [Required(ErrorMessage = "Refresh token is required")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    // For token-only responses (when you don't want to expose full session details)
    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; } // Seconds until expiration
    }

    // Session summary for listing sessions
    public class SessionSummaryDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUsedAt { get; set; }
        public bool IsActive { get; set; }
    }


}
