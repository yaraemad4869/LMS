using LearningManagementSystem.IServices;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace LearningManagementSystem.Filters
{
    public class LogActionFilter : IAsyncActionFilter
    {
        private readonly ILoggingService _loggingService;

        public LogActionFilter(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executedContext = await next();

            // Extract user information
            var userId=context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = context.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value ?? "Anonymous";

            // Create action description
            var controllerName = context.ActionDescriptor.RouteValues["controller"];
            var actionName = context.ActionDescriptor.RouteValues["action"];
            var action = $"{controllerName}.{actionName}";

            // Get additional info
            var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = context.HttpContext.Request.Headers["User-Agent"].ToString();

            // Log the action
            await _loggingService.LogAsync(userId, userName, action, null, ipAddress, userAgent);
        }
    }
}
