using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class User : Common
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordBackdoor { get; set; } // ONLY FOR DEV PURPOSES, DO NOT COMMIT TO PROD
        public string PasswordHash { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public int LoginAttempts { get; set; }
        public DateTime? LastFailedLogin { get; set; }
        public bool IsLocked { get; set; }
    }

    public class UserDTO
    {
        public string Username { get; set; }
        public string Email { get; set; }
    }

    public class UserSummaryDTO
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserProfileDTO
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class RegisterUserDTO
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*\W).{8,32}$",
            ErrorMessage = "Password must be 8–32 chars and include upper, lower, digit, and special character")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string PasswordConfirm { get; set; }
    }

    public class LoginUserDTO
    {
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }

    public class UpdateUserInfoDTO
    {
        public string Username { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
    }

    public class UpdateUserPasswordDTO
    {
        [Required(ErrorMessage = "Your current password is required")]
        public string CurrentPassword { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*\W).{8,32}$",
            ErrorMessage = "Password must be 8–32 chars and include upper, lower, digit, and special character")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Password confirmation is required")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string PasswordConfirm { get; set; }
    }
}