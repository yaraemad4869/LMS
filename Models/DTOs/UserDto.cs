namespace LearningManagementSystem.Models.DTOs
{
    public class UserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; }
        public virtual List<Course> Courses { get; set; } = new List<Course>();

    }
}
