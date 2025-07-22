using LearningManagementSystem.Data.Enum;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace LearningManagementSystem.Models
{
    public class Course
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public decimal Price { get; set; }
        [ForeignKey("Instructor")]
        public int InstructorId { get; set; }
        public virtual User? Instructor { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsPublished { get; set; } = false;
        public CourseLevel Level { get; set; } = CourseLevel.Beginner;
        public int DurationInMinutes { get; set; } = 0;
        public virtual List<Module> Modules { get; set; } = new List<Module>();
        public virtual List<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual List<Review> Reviews { get; set; } = new List<Review>();
        public virtual List<User> Students { get; set; } = new List<User>();
    }
}
