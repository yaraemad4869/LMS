using LearningManagementSystem.Data.Enum;

namespace LearningManagementSystem.Models.DTOs
{
    public class LectureDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ModuleId { get; set; }
        public int Order { get; set; }
        public LectureType Type { get; set; }
        public string ContentUrl { get; set; }
        public int DurationInMinutes { get; set; }
    }
}
