namespace LearningManagementSystem.Models
{
    public class Answer
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int QuestionId { get; set; }
        public virtual Question Question { get; set; }
        public bool IsCorrect { get; set; }
    }
}
