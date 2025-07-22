namespace LearningManagementSystem.Models.DTOs
{
    public class ModuleDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int CourseId { get; set; }
        public int Order { get; set; }
        public virtual List<Lecture> Lectures { get; set; }
    }
}
