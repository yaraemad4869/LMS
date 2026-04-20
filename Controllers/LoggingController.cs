using LearningManagementSystem.IServices;
using LearningManagementSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoggingController : ControllerBase
    {
        private readonly ILoggingService _loggingService;

        public LoggingController(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        [HttpPost("log")]
        public async Task<IActionResult> LogActivity([FromBody] LogEntry request)
        {
            await _loggingService.LogAsync(
                request.UserId,
                request.Username,
                request.UserRole,
                request.Action,
                request.EntityId,
                request.EntityName,
                request.Details,
                request.IpAddress
            );

            return Ok("Activity logged successfully.");
        }

        [HttpGet("logs")]
        public async Task<IActionResult> GetLogs([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var logs = await _loggingService.GetLogsAsync(startDate, endDate);
            return Ok(logs);
        }

        [HttpGet("logs/user/{userId}")]
        public async Task<IActionResult> GetUserLogs(string userId)
        {
            var logs = await _loggingService.GetUserLogsAsync(userId);
            return Ok(logs);
        }
    }
}
