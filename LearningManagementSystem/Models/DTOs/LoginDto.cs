using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.DTOs
{
    public class LoginDto
    {
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{8,}$",
            ErrorMessage = "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, and one number.")]
        public string Password { get; set; }
    }
}
