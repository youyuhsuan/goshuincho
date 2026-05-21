using System.ComponentModel.DataAnnotations;

namespace backend.DTOs.Requests
{
    public class RegisterRequest
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [MaxLength(500)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, and one number.")]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
    }

    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [MaxLength(500)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, and one number.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}