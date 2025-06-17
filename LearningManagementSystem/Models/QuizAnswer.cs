namespace LearningManagementSystem.Models
{
    public class QuizAnswer
    {
        public int Id { get; set; }
        public int QuizAttemptId { get; set; }
        public virtual QuizAttempt QuizAttempt { get; set; }
        public int QuestionId { get; set; }
        public virtual Question Question { get; set; }
        public int SelectedAnswerId { get; set; }
        public virtual Answer SelectedAnswer { get; set; }
    }
}
