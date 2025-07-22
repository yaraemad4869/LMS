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
    public class QuizzesController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<QuizzesController> _logger;
        private readonly ILoggingService _loggingService;
        private readonly IMemoryCache _cache;
        private readonly string _quizzesCacheKey = "QuizzesCache";
        private readonly string _quizzeIdCacheKey = "QuizIdCache";

        public QuizzesController(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ILoggingService loggingService, ILogger<QuizzesController> logger, UserManager<User> userManager, IMemoryCache cache)
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
                if (_cache.TryGetValue(_quizzeIdCacheKey, out Quiz cachedQuiz))
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetById", cachedQuiz.Id.ToString(), "Quiz", $"Retrieved quiz({cachedQuiz.Id}) from cache", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    return Ok(cachedQuiz);
                }
                else
                {
                    var quiz= await _unitOfWork.Quizzes.GetByIdAsync(id);
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5)) // Cache expires if not accessed for 5 mins
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))  // Cache expires after 1 hour
                .SetPriority(CacheItemPriority.Normal);

                    // Save data in cache
                    _cache.Set(_quizzeIdCacheKey, quiz, cacheEntryOptions);
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetById", quiz.Id.ToString(), "Quiz", $"Retrieved quiz({quiz.Id}) from database", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    return Ok(quiz);
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "GetById", id.ToString(), "Quiz", $"Error getting quiz({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error getting quiz{id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize("Instructor")]
        public async Task<IActionResult> Post(Quiz quiz)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            if (user.Courses.Any(c => c.Modules.Any(m => m.Lectures.Any(l=>l.Id==quiz.LectureId))))
            {
                try
                {
                    if (quiz== null)
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Post", quiz.Id.ToString(), "Quiz", $"There is no quiz", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                        return BadRequest("There is no quiz");
                    }
                    await _unitOfWork.Quizzes.AddAsync(_mapper.Map<Quiz>(quiz));
                    _cache.Remove(_quizzesCacheKey);
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Post", quiz.Id.ToString(), "Quiz", $"Posted new quiz", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    return CreatedAtAction(nameof(Post), quiz);
                }
                catch (Exception ex)
                {
                    await _loggingService.LogAsync("System", "System", "System", "Post", "", "Quiz", $"Error posting new quiz", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    _logger.LogError(ex, $"Error posting quiz");
                    return StatusCode(500, "Internal server error");
                }
            }
            return Forbid("Not Allowed To Post The Content");
        }

        [HttpPut("{id}")]
        [Authorize("Instructor")]
        public async Task<IActionResult> Put(int id, Quiz quiz)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            if (user.Courses.Any(c => c.Modules.Any(m => m.Lectures.Any(l => l.Id == quiz.LectureId))))
            {
                try
                {
                    if (quiz== null)
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", quiz.Id.ToString(), "Quiz", $"There is no quiz", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                        return BadRequest("There is no quiz");
                    }
                    else if (id != quiz.Id)
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", quiz.Id.ToString(), "Quiz", $"QuizID mismatch", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                        return BadRequest("QuizID mismatch");
                    }
                    var existingQuiz= await _unitOfWork.Quizzes.GetByIdAsync(id);
                    if (existingQuiz== null)
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", quiz.Id.ToString(), "Quiz", $"Quiznot found", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                        return NotFound("Quiznot found");
                    }
                    await _unitOfWork.Quizzes.Update(quiz);
                    _cache.Remove(_quizzesCacheKey);
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", quiz.Id.ToString(), "Quiz", $"Put quiz({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    return Ok(quiz);
                }

                catch (Exception ex)
                {
                    await _loggingService.LogAsync("System", "System", "System", "Put", quiz.Id.ToString(), "Quiz", $"Error putting quiz", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    _logger.LogError(ex, $"Error putting quiz({quiz.Id})");
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
                var quiz= await _unitOfWork.Quizzes.GetByIdAsync(id);
                if (User.IsInRole("Admin") || user.Courses.Any(c => c.Modules.Any(m => m.Lectures.Any(l => l.Id == quiz.LectureId))))
                {

                    if (quiz== null)
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", quiz.Id.ToString(), "Quiz", $"There is no quiz", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                        return BadRequest("There is no quiz");
                    }
                    await _unitOfWork.Quizzes.Remove(quiz);
                    _cache.Remove(_quizzesCacheKey);
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", quiz.Id.ToString(), "Quiz", $"Delete quiz({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is deleting quiz{id}");

                    return Ok("QuizDeleted: \n" + _mapper.Map<Quiz>(quiz));
                }
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", id.ToString(), "Quiz", $"does not delete quiz({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogWarning($"Unauthorized access attempt by user {user?.FullName} with ID {user?.Id} to delete quiz{id}");

                return Forbid("Not Allowed To Delete The Content");

            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Delete", id.ToString(), "Quiz", $"Error deleting quiz", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error deleting quiz({id})");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("Lecture/{lectureId}")]
        [Authorize]
        public async Task<IActionResult> GetQuizByLecture(int lectureId)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            if (user.Courses.Any(c => c.Modules.Any(m => m.Lectures.Any(l => l.Id == lectureId))))
            {
                try
                {
                    var quiz = await _unitOfWork.Quizzes.GetQuizByLectureIdAsync(lectureId);
                    if (quiz == null)
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetQuizByLecture", lectureId.ToString(), "Quiz", $"No quiz found for lecture ({lectureId})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                        _logger.LogInformation($"{user.FullName} with id {user.Id} is getting quiz of lecture {lectureId} that are 0");

                        return NotFound("No quiz found for this lecture");
                    }
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetQuizByLecture", lectureId.ToString(), "Quiz", $"Retrieved quiz for lecture ({lectureId})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting quiz of lecture {lectureId}");

                    return Ok(_mapper.Map<Quiz>(quiz));
                }
                catch (Exception ex)
                {
                    await _loggingService.LogAsync("System", "System", "System", "GetQuizzesByLecture", lectureId.ToString(), "Quiz", $"Error getting quizzes for lecture ({lectureId})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogError(ex, $"Error getting quizzes for lecture {lectureId}");
                    return StatusCode(500, "Internal server error");
                }
            }
            return Forbid("Not Allowed To Get The Content");

        }
    }
}
