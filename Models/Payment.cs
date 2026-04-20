using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LearningManagementSystem.Data.Enum;

namespace LearningManagementSystem.Models
{
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int StudentId { get; set; }
        public virtual User Student { get; set; }
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
        public decimal Amount { get; set; }
        public virtual DateTime PaymentDate { get; set; }
        public string TransactionId { get; set; }
        public PaymentStatus Status { get; set; }
    }
}
