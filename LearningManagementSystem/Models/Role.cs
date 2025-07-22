using Microsoft.AspNetCore.Identity;

namespace LearningManagementSystem.Models
{
    public class Role : IdentityRole<int>
    {
        public virtual ICollection<User> Users { get; set; }
    }
}
