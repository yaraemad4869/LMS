namespace LearningManagementSystem.Models
{
    public class Certificate
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public virtual Enrollment Enrollment { get; set; }
        public DateTime IssuedDate { get; set; }
        public string CertificateUrl { get; set; }
        public string VerificationCode { get; set; }
    }
}
