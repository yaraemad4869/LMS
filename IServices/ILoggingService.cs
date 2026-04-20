using LearningManagementSystem.Models;

namespace LearningManagementSystem.IServices
{
    public interface ILoggingService
    {
        Task LogAsync(string userId, string userName, string userRole, string action, string entityId, string entityName, string details = null, string ipAddress = null, string userAgent = null);
        Task<IEnumerable<LogEntry>> GetLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 50);
        Task<LogEntry> GetLogByIdAsync(int id);
        Task<IEnumerable<LogEntry>> GetUserLogsAsync(string userId);
    }
}
