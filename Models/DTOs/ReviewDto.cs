using System.ComponentModel.DataAnnotations;

namespace LearningManagementSystem.Models.DTOs
{
    public class ReviewDto
    {
        public string CourseName { get; set; }
        public string StudentName { get; set; }
        [MinLength(1)]
        [MaxLength(5)]
        public int Rating { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
