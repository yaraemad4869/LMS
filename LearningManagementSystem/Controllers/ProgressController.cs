using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using LearningManagementSystem.Extensions;
using LearningManagementSystem.IServices;
using LearningManagementSystem.IUOW;
using LearningManagementSystem.Models;
using LearningManagementSystem.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LearningManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgressesController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProgressesController> _logger;
        private readonly IMapper _mapper;
        private readonly ILoggingService _loggingService;
        private readonly IMemoryCache _cache;
        private readonly string _progressesCacheKey = "ProgressesCache";
        private readonly string _progressIdCacheKey = "ProgressIdCache";

        public ProgressesController(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILoggingService loggingService, ILogger<ProgressesController> logger, UserManager<User> userManager, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
            _cache = cache;
            _loggingService = loggingService;
            _httpContextAccessor = httpContextAccessor;
        }
        [HttpGet]
        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> GetAllProgressesByEnrollment(int enrollmentId)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            if (User.IsInRole("Admin") || user.Enrollments.Any(e=>e.Id==enrollmentId))
            {
                try
                {
                    var progresses = await _unitOfWork.Progress.GetProgressByEnrollmentAsync(enrollmentId);
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetAllProgressesByEnrollment", "", "Progress", $"Retrieved progress of enrollment ({enrollmentId})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} in role {user.UserRole.Name} is getting progress of enrollment ({enrollmentId})");
                    return Ok(progresses);
                }
                catch (Exception ex)
                {
                    await _loggingService.LogAsync("System", "System", "System", "GetAllProgressesByEnrollment", "", "Progress", $"Error getting progress of enrollment ({enrollmentId})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogError(ex, $"Error getting progress of enrollment ({enrollmentId})");
                    return StatusCode(500, "Internal server error");
                }
            }
            return Forbid("Not Allowed To Get The Content");

        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);

            try
            {
                if (_cache.TryGetValue(_progressIdCacheKey, out Progress? cachedProgress))
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole?.Name, "GetById", cachedProgress?.Id.ToString(), "Progress", $"Retrieved progress ({cachedProgress?.Id}) from cache", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} in role {user.UserRole.Name} is getting a progress with id {id} from cache");
                    return Ok(_mapper.Map<IEnumerable<Progress>>(cachedProgress));
                }
                else
                {
                    var progress = await _unitOfWork.Progress.GetByIdAsync(id);
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5)) // Cache expires if not accessed for 5 mins
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))  // Cache expires after 1 hour
                .SetPriority(CacheItemPriority.Normal);

                    // Save data in cache
                    _cache.Set(_progressIdCacheKey, progress, cacheEntryOptions);
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "System", user?.UserRole.Name, "GetById", progress.Id.ToString(), "Progress", $"Retrieved progress ({cachedProgress.Id}) from database", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} in role {user.UserRole.Name} is getting a progress with id {id} from database");
                    return Ok(_mapper.Map<Progress>(progress));
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "GetById", id.ToString(), "Progress", $"Error getting progress ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error getting progress {id}");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Put(int id, Progress progress)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            try
            {
                if (progress == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", progress.Id.ToString(), "Progress", $"There is no progress", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.FullName ?? "Unknown"} doesn't give progress.");
                    return BadRequest("There is no progress");
                }
                else if (id != progress.Id)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", progress.Id.ToString(), "Progress", $"Progress ID mismatch", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.FullName ?? "Unknown"} attempted to update progress with id {id} that doesn't match with progress given.");
                    return BadRequest("Progress ID mismatch");
                }
                var existingProgress = await _unitOfWork.Progress.GetByIdAsync(id);
                if (existingProgress == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", progress.Id.ToString(), "Progress", $"Progress not found", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.FullName ?? "Unknown"} attempted to update progress with id {id} that doesn't exist.");
                    return NotFound("Progress not found");
                }
                await _unitOfWork.Progress.Update(progress);
                _cache.Remove(_progressesCacheKey);
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", progress.Id.ToString(), "Progress", $"Put progress ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogInformation($"{user.FullName} with id {user.Id} in role {user.UserRole.Name} updates a progress with id {id}");
                return Ok(_mapper.Map<IEnumerable<Progress>>(progress));
            }

            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Put", progress.Id.ToString(), "Progress", $"Error putting progress", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error putting progress ({progress.Id})");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            try
            {
                var progress = await _unitOfWork.Progress.GetByIdAsync(id);

                if (progress == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", progress.Id.ToString(), "Progress", $"There is no progress", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.FullName ?? "Unknown"} attempted to delete progress with id {id} that doesn't exist.");
                    return BadRequest("There is no progress");
                }
                await _unitOfWork.Progress.Remove(progress);
                _cache.Remove(_progressesCacheKey);
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", progress.Id.ToString(), "Progress", $"Delete progress ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogInformation($"{user.FullName} with id {user.Id} in role {user.UserRole.Name} is deleting a progress with id {id} from database");
                return Ok(_mapper.Map<Progress>(progress));
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Delete", id.ToString(), "Progress", $"Error deleting progress", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error deleting progress ({id})");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
