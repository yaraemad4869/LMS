using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LearningManagementSystem.Data.Enum;

namespace LearningManagementSystem.Models
{
    public class Enrollment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int StudentId { get; set; }
        public virtual User Student { get; set; }
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;
        public DateTime? CompletionDate { get; set; }
        public EnrollmentStatus Status { get; set; }
        public bool CertificateIssued { get; set; } = false;
        public virtual List<Progress> Progress { get; set; }
    }
}
