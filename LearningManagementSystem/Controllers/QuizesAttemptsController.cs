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
    public class QuizAttemptsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<QuizAttemptsController> _logger;
        private readonly ILoggingService _loggingService;
        private readonly IMemoryCache _cache;
        private readonly string _quizAttemptsCacheKey = "QuizAttemptsCache";
        private readonly string _quizAttemptzeIdCacheKey = "QuizAttemptIdCache";

        public QuizAttemptsController(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ILoggingService loggingService, ILogger<QuizAttemptsController> logger, UserManager<User> userManager, IMemoryCache cache)
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
                if (_cache.TryGetValue(_quizAttemptzeIdCacheKey, out QuizAttempt cachedQuizAttempt))
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetById", cachedQuizAttempt.Id.ToString(), "QuizAttempt", $"Retrieved quizAttempt({cachedQuizAttempt.Id}) from cache", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    return Ok(cachedQuizAttempt);
                }
                else
                {
                    var quizAttempt = await _unitOfWork.QuizAttempts.GetByIdAsync(id);
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5)) // Cache expires if not accessed for 5 mins
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))  // Cache expires after 1 hour
                .SetPriority(CacheItemPriority.Normal);

                    // Save data in cache
                    _cache.Set(_quizAttemptzeIdCacheKey, quizAttempt, cacheEntryOptions);
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetById", quizAttempt.Id.ToString(), "QuizAttempt", $"Retrieved quizAttempt({quizAttempt.Id}) from database", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    return Ok(quizAttempt);
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "GetById", id.ToString(), "QuizAttempt", $"Error getting quizAttempt({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error getting quizAttempt{id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize("Instructor")]
        public async Task<IActionResult> Post(QuizAttempt quizAttempt)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            if (user.Courses.Any(c => c.Modules.Any(m => m.Lectures.Any(l => l.Id == quizAttempt.Quiz.LectureId))))
            {
                try
                {
                    if (quizAttempt == null)
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Post", quizAttempt.Id.ToString(), "QuizAttempt", $"There is no quizAttempt", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                        return BadRequest("There is no quizAttempt");
                    }
                    await _unitOfWork.QuizAttempts.AddAsync(_mapper.Map<QuizAttempt>(quizAttempt));
                    _cache.Remove(_quizAttemptsCacheKey);
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Post", quizAttempt.Id.ToString(), "QuizAttempt", $"Posted new quizAttempt", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    return CreatedAtAction(nameof(Post), quizAttempt);
                }
                catch (Exception ex)
                {
                    await _loggingService.LogAsync("System", "System", "System", "Post", "", "QuizAttempt", $"Error posting new quizAttempt", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    _logger.LogError(ex, $"Error posting quizAttempt");
                    return StatusCode(500, "Internal server error");
                }
            }
            return Forbid("Not Allowed To Post The Content");
        }

        [HttpPut("{id}")]
        [Authorize("Instructor")]
        public async Task<IActionResult> Put(int id, QuizAttempt quizAttempt)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            if (user.Courses.Any(c => c.Modules.Any(m => m.Lectures.Any(l => l.Id == quizAttempt.Quiz.LectureId))))
            {
                try
                {
                    if (quizAttempt == null)
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", quizAttempt.Id.ToString(), "QuizAttempt", $"There is no quizAttempt", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                        return BadRequest("There is no quizAttempt");
                    }
                    else if (id != quizAttempt.Id)
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", quizAttempt.Id.ToString(), "QuizAttempt", $"QuizAttemptID mismatch", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                        return BadRequest("QuizAttemptID mismatch");
                    }
                    var existingQuizAttempt = await _unitOfWork.QuizAttempts.GetByIdAsync(id);
                    if (existingQuizAttempt == null)
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", quizAttempt.Id.ToString(), "QuizAttempt", $"QuizAttemptnot found", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                        return NotFound("QuizAttemptnot found");
                    }
                    await _unitOfWork.QuizAttempts.Update(quizAttempt);
                    _cache.Remove(_quizAttemptsCacheKey);
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", quizAttempt.Id.ToString(), "QuizAttempt", $"Put quizAttempt({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    return Ok(quizAttempt);
                }

                catch (Exception ex)
                {
                    await _loggingService.LogAsync("System", "System", "System", "Put", quizAttempt.Id.ToString(), "QuizAttempt", $"Error putting quizAttempt", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    _logger.LogError(ex, $"Error putting quizAttempt({quizAttempt.Id})");
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
                var quizAttempt = await _unitOfWork.QuizAttempts.GetByIdAsync(id);
                if (User.IsInRole("Admin") || user.Courses.Any(c => c.Modules.Any(m => m.Lectures.Any(l => l.Id == quizAttempt.Quiz.LectureId))))
                {

                    if (quizAttempt == null)
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", quizAttempt.Id.ToString(), "QuizAttempt", $"There is no quizAttempt", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                        return BadRequest("There is no quizAttempt");
                    }
                    await _unitOfWork.QuizAttempts.Remove(quizAttempt);
                    _cache.Remove(_quizAttemptsCacheKey);
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", quizAttempt.Id.ToString(), "QuizAttempt", $"Delete quizAttempt({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is deleting quizAttempt{id}");

                    return Ok("QuizAttemptDeleted: \n" + _mapper.Map<QuizAttempt>(quizAttempt));
                }
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", id.ToString(), "QuizAttempt", $"does not delete quizAttempt({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogWarning($"Unauthorized access attempt by user {user?.FullName} with ID {user?.Id} to delete quizAttempt{id}");

                return Forbid("Not Allowed To Delete The Content");

            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Delete", id.ToString(), "QuizAttempt", $"Error deleting quizAttempt", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error deleting quizAttempt({id})");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("Student/{studentId}/Quiz/{quizId}")]
        [Authorize]
        public async Task<IActionResult> GetAttemptsByStudentAndQuizAsync(int studentId, int quizId)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            if (user.Courses.Any(c => c.Modules.Any(m => m.Lectures.Any(l => l.Id == studentId))))
            {
                try
                {
                    var quizAttempt = await _unitOfWork.QuizAttempts.GetAttemptsByStudentAsync(studentId, quizId);
                    if (quizAttempt == null)
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetAttemptsByStudentAsync", "", "QuizAttempt", $"No quiz attempt found for  student {studentId} and quiz {quizId}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                        _logger.LogInformation($"{user.FullName} with id {user.Id} is getting quiz attempt of student {studentId} and quiz {quizId} that are 0");

                        return NotFound("No quizAttempt found for this lecture");
                    }
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetAttemptsByStudentAsync", "", "QuizAttempt", $"Retrieved quiz attempt for  student {studentId} and quiz {quizId}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting quiz attempt of student {studentId} and quiz {quizId}");

                    return Ok(_mapper.Map<QuizAttempt>(quizAttempt));
                }
                catch (Exception ex)
                {
                    await _loggingService.LogAsync("System", "System", "System", "GetQuizAttemptsByLecture", "", "QuizAttempt", $"Error getting quiz attempts for  student {studentId} and quiz {quizId}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogError(ex, $"Error getting quiz attempts for  student {studentId} and quiz {quizId}");
                    return StatusCode(500, "Internal server error");
                }
            }
            return Forbid("Not Allowed To Get The Content");

        }
    }
}
