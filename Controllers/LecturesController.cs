using System.Reflection;
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
namespace LearningManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LecturesController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<LecturesController> _logger;
        private readonly ILoggingService _loggingService;
        private readonly IMemoryCache _cache;
        private readonly string _lecturesCacheKey = "LecturesCache";
        private readonly string _lectureIdCacheKey = "LectureIdCache";

        public LecturesController(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ILoggingService loggingService, ILogger<LecturesController> logger, UserManager<User> userManager, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _cache = cache;
            _loggingService = loggingService;
            _httpContextAccessor = httpContextAccessor;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);

            try
            {
                if (_cache.TryGetValue(_lectureIdCacheKey, out Lecture cachedLecture))
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetById", cachedLecture.Id.ToString(), "Lecture", $"Retrieved lecture ({cachedLecture.Id}) from cache", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    return Ok(_mapper.Map<LectureDto>(cachedLecture));
                }
                else
                {
                    var lecture = await _unitOfWork.Lectures.GetByIdAsync(id);
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5)) // Cache expires if not accessed for 5 mins
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))  // Cache expires after 1 hour
                .SetPriority(CacheItemPriority.Normal);

                    // Save data in cache
                    _cache.Set(_lectureIdCacheKey, lecture, cacheEntryOptions);
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetById", lecture.Id.ToString(), "Lecture", $"Retrieved lecture ({lecture.Id}) from database", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    return Ok(_mapper.Map<LectureDto>(lecture));
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "GetById", id.ToString(), "Lecture", $"Error getting lecture ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error getting lecture {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize("Instructor")]
        public async Task<IActionResult> Post(LectureDto lecture)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            if (user.Courses.Any(c => c.Modules.Any(m => m.Id == lecture.ModuleId)))
            {
                try
                {
                    if (lecture == null)
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Post", lecture.Id.ToString(), "Lecture", $"There is no lecture", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                        return BadRequest("There is no lecture");
                    }
                    await _unitOfWork.Lectures.AddAsync(_mapper.Map<Lecture>(lecture));
                    _cache.Remove(_lecturesCacheKey);
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Post", lecture.Id.ToString(), "Lecture", $"Posted new lecture", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    return CreatedAtAction(nameof(Post), lecture);
                }
                catch (Exception ex)
                {
                    await _loggingService.LogAsync("System", "System", "System", "Post", "", "Lecture", $"Error posting new lecture", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    _logger.LogError(ex, $"Error posting lecture");
                    return StatusCode(500, "Internal server error");
                }
            }
            return Forbid("Not Allowed To Post The Content");
        }

        [HttpPut("{id}")]
        [Authorize("Instructor")]
        public async Task<IActionResult> Put(int id, Lecture lecture)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            if (user.Courses.Any(c => c.Modules.Any(m => m.Lectures.Any(l => l.Id == id))))
            {
                try
                {
                    if (lecture == null)
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", lecture.Id.ToString(), "Lecture", $"There is no lecture", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                        return BadRequest("There is no lecture");
                    }
                    else if (id != lecture.Id)
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", lecture.Id.ToString(), "Lecture", $"Lecture ID mismatch", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                        return BadRequest("Lecture ID mismatch");
                    }
                    var existingLecture = await _unitOfWork.Lectures.GetByIdAsync(id);
                    if (existingLecture == null)
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", lecture.Id.ToString(), "Lecture", $"Lecture not found", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                        return NotFound("Lecture not found");
                    }
                    await _unitOfWork.Lectures.Update(lecture);
                    _cache.Remove(_lecturesCacheKey);
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", lecture.Id.ToString(), "Lecture", $"Put lecture ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    return Ok(lecture);
                }

                catch (Exception ex)
                {
                    await _loggingService.LogAsync("System", "System", "System", "Put", lecture.Id.ToString(), "Lecture", $"Error putting lecture", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    _logger.LogError(ex, $"Error putting lecture ({lecture.Id})");
                    return StatusCode(500, "Internal server error");
                }
            }
            return Forbid("Not Allowed To Update The Content");

        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Delete(int id)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            try
            {
                var lecture = await _unitOfWork.Lectures.GetByIdAsync(id);
                if (User.IsInRole("Admin") || user.Courses.Any(c => c.Modules.Any(m => m.Lectures.Any(l => l.Id==id))))
                {

                    if (lecture == null)
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", lecture.Id.ToString(), "Lecture", $"There is no lecture", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                        return BadRequest("There is no lecture");
                    }
                    await _unitOfWork.Lectures.Remove(lecture);
                    _cache.Remove(_lecturesCacheKey);
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", lecture.Id.ToString(), "Lecture", $"Delete lecture ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is deleting lecture {id}");

                    return Ok("Lecture Deleted: \n" + _mapper.Map<LectureDto>(lecture));
                }
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", id.ToString(), "Lecture", $"does not delete lecture ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogWarning($"Unauthorized access attempt by user {user?.FullName} with ID {user?.Id} to delete lecture {id}");

                return Forbid("Not Allowed To Delete The Content");

            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Delete", id.ToString(), "Lecture", $"Error deleting lecture", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error deleting lecture ({id})");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("Module/{moduleId}")]
        [Authorize]
        public async Task<IActionResult> GetLecturesByModule(int moduleId)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            if (user.Courses.Any(c => c.Modules.Any(m => m.Lectures.Any(l => l.ModuleId == moduleId))))
            {
                try
                {
                    var lectures = await _unitOfWork.Lectures.GetLecturesByModuleAsync(moduleId);
                    if (lectures == null || !lectures.Any())
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetLecturesByModule", moduleId.ToString(), "Lecture", $"No lectures found for module ({moduleId})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                        _logger.LogInformation($"{user.FullName} with id {user.Id} is getting lectures of module {moduleId} that are 0");

                        return NotFound("No lectures found for this module");
                    }
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetLecturesByModule", moduleId.ToString(), "Lecture", $"Retrieved lectures for module ({moduleId})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting lectures of module {moduleId}");

                    return Ok(_mapper.Map<LectureDto>(lectures));
                }
                catch (Exception ex)
                {
                    await _loggingService.LogAsync("System", "System", "System", "GetLecturesByModule", moduleId.ToString(), "Lecture", $"Error getting lectures for module ({moduleId})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogError(ex, $"Error getting lectures for module {moduleId}");
                    return StatusCode(500, "Internal server error");
                }
            }
            return Forbid("Not Allowed To Get The Content");

        }
    }
}
