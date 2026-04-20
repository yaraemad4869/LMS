using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models
{
    public class Certificate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public virtual Enrollment Enrollment { get; set; }
        public DateTime IssuedDate { get; set; }
        public byte[] Document { get; set; }
        //public string VerificationCode { get; set; }
    }
}
