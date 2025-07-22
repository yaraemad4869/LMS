using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models
{
    public class Module
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
        public int Order { get; set; }
        public virtual List<Lecture> Lectures { get; set; }
    }
}
