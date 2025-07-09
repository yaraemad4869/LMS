using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using LearningManagementSystem.Extensions;
using LearningManagementSystem.IServices;
using LearningManagementSystem.IUOW;
using LearningManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LearningManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EnrollmentsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EnrollmentsController> _logger;
        private readonly ILoggingService _loggingService;
        private readonly IMemoryCache _cache;
        private readonly string _enrollmentsCacheKey = "EnrollmentsCache";
        private readonly string _enrollmentIdCacheKey = "EnrollmentIdCache";

        public EnrollmentsController(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ILoggingService loggingService, /*ILogger<EnrollmentsController> logger,*/ UserManager<User> userManager, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            //_logger = logger;
            _userManager = userManager;
            _cache = cache;
            _loggingService = loggingService;
            _httpContextAccessor = httpContextAccessor;
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            if (user == null)
            {
                return Unauthorized("User not found");
            }
            //_logger.LogInformation($"{user.FullName} with id {user.Id} is getting all enrollments");
            try
            {
                if (_cache.TryGetValue(_enrollmentsCacheKey, out IEnumerable<Enrollment> cachedEnrollments))
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetEnrollments", "", "Enrollment", $"Retrieved {cachedEnrollments.Count()} enrollments from cache", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    return Ok(cachedEnrollments);
                }
                else
                {
                    var enrollments = await _unitOfWork.Enrollments.GetAllAsync();
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5)) // Cache expires if not accessed for 5 mins
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))  // Cache expires after 1 hour
                .SetPriority(CacheItemPriority.Normal);

                    // Save data in cache
                    _cache.Set(_enrollmentsCacheKey, enrollments, cacheEntryOptions);

                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetEnrollments", "", "Enrollment", $"Retrieved {cachedEnrollments.Count()} enrollments from database", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    return Ok(enrollments);
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "GetEnrollments", "", "Enrollment", $"Error getting enrollments", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, "Error getting enrollments");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);

            try
            {
                if (_cache.TryGetValue(_enrollmentIdCacheKey, out Enrollment cachedEnrollment))
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetById", cachedEnrollment.Id.ToString(), "Enrollment", $"Retrieved enrollment ({cachedEnrollment.Id}) from cache", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    return Ok(cachedEnrollment);
                }
                else
                {
                    var enrollments = await _unitOfWork.Enrollments.GetAllAsync();
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5)) // Cache expires if not accessed for 5 mins
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))  // Cache expires after 1 hour
                .SetPriority(CacheItemPriority.Normal);

                    // Save data in cache
                    _cache.Set(_enrollmentIdCacheKey, enrollments, cacheEntryOptions);
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetById", cachedEnrollment.Id.ToString(), "Enrollment", $"Retrieved enrollment ({cachedEnrollment.Id}) from database", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    return Ok(enrollments);
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "GetById", id.ToString(), "Enrollment", $"Error getting enrollment ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error getting enrollment {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post(Enrollment enrollment)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            try
            {
                if (enrollment == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Post", enrollment.Id.ToString(), "Enrollment", $"There is no enrollment", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    return BadRequest("There is no enrollment");
                }
                await _unitOfWork.Enrollments.AddAsync(enrollment);
                _cache.Remove(_enrollmentsCacheKey);
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Post", enrollment.Id.ToString(), "Enrollment", $"Posted new enrollment", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                return CreatedAtAction(nameof(Post), new { id = enrollment.Id }, enrollment);
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Post", enrollment.Id.ToString(), "Enrollment", $"Error posting new enrollment", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error posting enrollment");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, Enrollment enrollment)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            try
            {
                if (enrollment == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", enrollment.Id.ToString(), "Enrollment", $"There is no enrollment", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    return BadRequest("There is no enrollment");
                }
                else if (id != enrollment.Id)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", enrollment.Id.ToString(), "Enrollment", $"Enrollment ID mismatch", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    return BadRequest("Enrollment ID mismatch");
                }
                var existingEnrollment = await _unitOfWork.Enrollments.GetByIdAsync(id);
                if (existingEnrollment == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", enrollment.Id.ToString(), "Enrollment", $"Enrollment not found", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    return NotFound("Enrollment not found");
                }
                await _unitOfWork.Enrollments.Update(enrollment);
                _cache.Remove(_enrollmentsCacheKey);
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", enrollment.Id.ToString(), "Enrollment", $"Put enrollment ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                return Ok(enrollment);
            }

            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Put", enrollment.Id.ToString(), "Enrollment", $"Error putting enrollment", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error putting enrollment ({enrollment.Id})");
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
                var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(id);

                if (enrollment == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", enrollment.Id.ToString(), "Enrollment", $"There is no enrollment", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    return BadRequest("There is no enrollment");
                }
                await _unitOfWork.Enrollments.Remove(enrollment);
                _cache.Remove(_enrollmentsCacheKey);
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", enrollment.Id.ToString(), "Enrollment", $"Delete enrollment ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                return Ok("Enrollment Deleted: \n" + enrollment);
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Delete", id.ToString(), "Enrollment", $"Error deleting enrollment", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error deleting enrollment ({id})");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
