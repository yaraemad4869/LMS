using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LearningManagementSystem.Data.Enum;
using Microsoft.AspNetCore.Identity;

namespace LearningManagementSystem.Models
{
    public class User : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }

        public override string PhoneNumber { get; set; }
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public override string Email { get; set; }
        public override string UserName { get; set; }

        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{8,}$",
            ErrorMessage = "Password must be at least 8 characters long, contain at least one uppercase letter, one lowercase letter, and one number.")]
        public string Password { get; set; }
        public string? GoogleId { get; set; }

        [ForeignKey("UserRole")]
        public virtual int RoleId { get; set; } = -3; // Default role ID for Student
        public virtual Role UserRole { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; } = true;
        public virtual List<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual List<Course> Courses { get; set; } = new List<Course>();
        public virtual List<Order> Orders { get; set; } = new List<Order>();
        public virtual List<Review> Reviews { get; set; } = new List<Review>();
    }
}
