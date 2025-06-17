using LearningManagementSystem.Data.@enum;

namespace LearningManagementSystem.Models
{
    public class Lecture
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ModuleId { get; set; }
        public virtual Module Module { get; set; }
        public int Order { get; set; }
        public LectureType Type { get; set; }
        public string ContentUrl { get; set; } 
        public int DurationInMinutes { get; set; } 
    }
}
