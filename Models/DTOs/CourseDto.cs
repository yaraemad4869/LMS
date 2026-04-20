using DocumentFormat.OpenXml.Wordprocessing;
using LearningManagementSystem.Data.Enum;

namespace LearningManagementSystem.Models.DTOs
{
    public class CourseDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public double AverageRating { get; set; } = 0.0;
        public decimal Price { get; set; }
        public int InstructorId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public bool IsPublished { get; set; } = false;
        public CourseLevel Level { get; set; } = CourseLevel.Beginner;
        public int DurationInMinutes { get; set; } = 0;
        public int NumOfStudents { get; set; } = 0;
    }
}
