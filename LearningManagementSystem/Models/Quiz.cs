﻿namespace LearningManagementSystem.Models
{
    public class Quiz
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int LectureId { get; set; }
        public virtual Lecture Lecture { get; set; }
        public virtual List<Question> Questions { get; set; }
        public int PassingScore { get; set; }
        public int TimeLimit { get; set; }
    }
}
