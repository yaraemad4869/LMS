namespace LearningManagementSystem.Models.DTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UserRoleName { get; set; }
    }
}
