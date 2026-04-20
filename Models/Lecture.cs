using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LearningManagementSystem.Data.Enum;

namespace LearningManagementSystem.Models
{
    public class Lecture
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
