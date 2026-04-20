using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningManagementSystem.Models
{
    public class LogEntry
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserId { get; set; }
        public virtual User User { get; set; }
        [MaxLength(255)]
        public string Username { get; set; } // Username of the user performing the action
        public string UserRole { get; set; }
        public DateTime Datetime { get; set; }
        [MaxLength(500)]
        public string Action { get; set; }
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public string Details { get; set; } // Additional details about the action
        public string IpAddress { get; set; } // IP address of the user
        public string UserAgent { get; set; } // User agent string for the request
    }
}
