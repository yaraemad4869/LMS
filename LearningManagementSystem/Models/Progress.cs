namespace LearningManagementSystem.Models
{
    public class Progress
    {
        public int Id { get; set; }
        public int EnrollmentId { get; set; }
        public virtual Enrollment Enrollment { get; set; }
        public int LectureId { get; set; }
        public virtual Lecture Lecture { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletionDate { get; set; }
    }
}
