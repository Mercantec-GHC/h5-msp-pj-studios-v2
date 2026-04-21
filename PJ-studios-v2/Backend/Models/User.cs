using System.ComponentModel.DataAnnotations;

namespace Backend.Models
{
    public class User : Common
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordBackdoor { get; set; } // ONLY FOR DEV PURPOSES, DO NOT COMMIT TO PROD
        public string PasswordHash { get; set; }
    }

    public class UserDTO
    {
        public string Username { get; set; }
        public string Email { get; set; }
    }

    public class RegisterUserDTO
    {
        [Required]
        public string Username { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string PasswordConfirm { get; set; }
    }
}
