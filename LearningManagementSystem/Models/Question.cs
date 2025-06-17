namespace LearningManagementSystem.Models
{
    public class Question
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public int QuizId { get; set; }
        public virtual Quiz Quiz { get; set; }
        public virtual List<Answer> Answers { get; set; }
    }
}
