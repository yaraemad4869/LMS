using LearningManagementSystem.IServices;

namespace LearningManagementSystem.Extensions
{
    public static class LoggingExtensions
    {
        public static async Task LogUserActionAsync(this ILoggingService loggingService, HttpContext httpContext, string action, string details = null)
        {
            var userId=httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userName = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Anonymous";
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();

            await loggingService.LogAsync(userId, userName, action, details, ipAddress, userAgent);
        }
    }
}
