using LearningManagementSystem.Data.@enum;

namespace LearningManagementSystem.Models
{
    public class AuthResponse
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public virtual UserRole Role { get; set; }
        public string Token { get; set; }
    }
}
