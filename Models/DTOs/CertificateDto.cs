namespace LearningManagementSystem.Models.DTOs
{
    public class CertificateDto
    {
        public int Id { get; set; }
        public string CourseName { get; set; }
        public string StudentName { get; set; }
        public DateTime IssuedDate { get; set; } = DateTime.UtcNow;

    }
}
