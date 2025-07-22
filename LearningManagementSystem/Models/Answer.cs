using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models
{
    public class Answer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Text { get; set; }
        public int QuestionId { get; set; }
        public virtual Question Question { get; set; }
        public bool IsCorrect { get; set; }
    }
}
