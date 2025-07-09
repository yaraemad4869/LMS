using LearningManagementSystem.Data;
using LearningManagementSystem.IServices;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoggingService> _logger;
        private readonly List<LogEntry> _logs=new();

        public LoggingService(ApplicationDbContext context, ILogger<LoggingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogAsync(string userId, string userName, string userRole, string action, string entityId, string entityName, string details = null, string ipAddress = null, string userAgent = null)
        {
            try
            {
                var logEntry = new LogEntry
                {
                    UserId = userId,
                    Username = userName,
                    UserRole = userRole,
                    Action = action,
                    EntityId = entityId,
                    EntityName = entityName,
                    Datetime = DateTime.UtcNow,
                    Details = details,
                    IpAddress = ipAddress,
                    UserAgent = userAgent
                };

                await _context.LogEntries.AddAsync(logEntry);
                await _context.SaveChangesAsync();
                _logs.Add(logEntry); // Store in memory for quick access if needed
                _logger.LogInformation($"User {userName} ({userId}) and Role {userRole} performed action {action} on {entityName} ({entityId}) from IP {ipAddress}.\nDetails: {details}");
            }
            catch (Exception ex)
            {
                // Log the exception but don't throw to prevent breaking the main application flow
                _logger.LogError(ex, "Failed to log action: {Action} for user: {UserId}", action, userId);
            }
        }

        public async Task<IEnumerable<LogEntry>> GetLogsAsync(DateTime? fromDate = null, DateTime? toDate = null, int page = 1, int pageSize = 50)
        {
            var query = _context.LogEntries.AsQueryable();


            if (fromDate.HasValue)
            {
                query = query.Where(l => l.Datetime >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                query = query.Where(l => l.Datetime <= toDate.Value);
            }

            return await query
                .OrderByDescending(l => l.Datetime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<IEnumerable<LogEntry>> GetUserLogsAsync(string userId)
        {
            return await _context.LogEntries
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.Datetime)
                .ToListAsync();
        }

        public async Task<LogEntry> GetLogByIdAsync(int id)
        {
            return await _context.LogEntries.FindAsync(id);
        }
    }
}
