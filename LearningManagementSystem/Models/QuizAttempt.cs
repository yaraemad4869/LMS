namespace LearningManagementSystem.Models
{
    public class QuizAttempt
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public virtual User Student { get; set; }
        public int QuizId { get; set; }
        public virtual Quiz Quiz { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int Score { get; set; }
        public bool Passed { get; set; }
        public virtual List<QuizAnswer> Answers { get; set; }
    }
}
