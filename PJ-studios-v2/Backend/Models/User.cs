using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class User : Common
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public List<RefreshToken> RefreshTokens { get; set; } = new();

        public int LoginAttempts { get; set; }
        public DateTime? LastFailedLogin { get; set; }
        public bool IsLocked { get; set; }
    }

    public class UserDTO
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class UserSummaryDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class UserProfileDTO
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class RegisterUserDTO
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*\W).{8,32}$",
            ErrorMessage = "Password must be 8–32 chars and include upper, lower, digit, and special character")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password))]
        public string PasswordConfirm { get; set; } = string.Empty;
    }

    public class LoginUserDTO
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateUserInfoDTO
    {
        public string Username { get; set; } = string.Empty;

        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateUserPasswordDTO
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*\W).{8,32}$",
            ErrorMessage = "Password must be 8–32 chars and include upper, lower, digit, and special character")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(Password))]
        public string PasswordConfirm { get; set; } = string.Empty;
    }

    public class DeleteUserDTO
    {
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}