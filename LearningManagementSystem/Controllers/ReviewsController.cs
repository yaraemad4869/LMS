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
    public class ReviewsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReviewsController> _logger;
        private readonly ILoggingService _loggingService;
        private readonly IMemoryCache _cache;
        private readonly string _reviewsCacheKey = "ReviewsCache";
        private readonly string _reviewIdCacheKey = "ReviewIdCache";

        public ReviewsController(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ILoggingService loggingService, /*ILogger<ReviewsController> logger,*/ UserManager<User> userManager, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            //_logger = logger;
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
                if (_cache.TryGetValue(_reviewIdCacheKey, out Review cachedReview))
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetById", cachedReview.Id.ToString(), "Review", $"Retrieved review ({cachedReview.Id}) from cache", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    return Ok(cachedReview);
                }
                else
                {
                    var reviews = await _unitOfWork.Reviews.GetAllAsync();
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5)) // Cache expires if not accessed for 5 mins
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))  // Cache expires after 1 hour
                .SetPriority(CacheItemPriority.Normal);

                    // Save data in cache
                    _cache.Set(_reviewIdCacheKey, reviews, cacheEntryOptions);
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetById", cachedReview.Id.ToString(), "Review", $"Retrieved review ({cachedReview.Id}) from database", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    return Ok(reviews);
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "GetById", id.ToString(), "Review", $"Error getting review ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error getting review {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post(Review review)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            try
            {
                if (review == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Post", review.Id.ToString(), "Review", $"There is no review", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    return BadRequest("There is no review");
                }
                await _unitOfWork.Reviews.AddAsync(review);
                _cache.Remove(_reviewsCacheKey);
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Post", review.Id.ToString(), "Review", $"Posted new review", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                return CreatedAtAction(nameof(Post), new { id = review.Id }, review);
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Post", review.Id.ToString(), "Review", $"Error posting new review", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error posting review");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(int id, Review review)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            if(review.StudentId != user.Id)
            {
                await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "Put", review.Id.ToString(), "Review", $"Unauthorized attempt to update review ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogWarning($"Unauthorized attempt by user {user?.FullName} with ID {user?.Id} to update review ({id})");
                return Unauthorized("You are not authorized to update this review");
            }
            try
            {
                if (review == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", review.Id.ToString(), "Review", $"There is no review", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    return BadRequest("There is no review");
                }
                else if (id != review.Id)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", review.Id.ToString(), "Review", $"Review ID mismatch", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    return BadRequest("Review ID mismatch");
                }
                var existingReview = await _unitOfWork.Reviews.GetByIdAsync(id);
                if (existingReview == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", review.Id.ToString(), "Review", $"Review not found", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    return NotFound("Review not found");
                }
                await _unitOfWork.Reviews.Update(review);
                _cache.Remove(_reviewsCacheKey);
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", review.Id.ToString(), "Review", $"Put review ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                return Ok(review);
            }

            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Put", review.Id.ToString(), "Review", $"Error putting review", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error putting review ({review.Id})");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            if(user.Id != id && user.UserRole.Name=="Admin")
            {
                await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "Delete", id.ToString(), "Review", $"Unauthorized attempt to delete review ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogWarning($"Unauthorized attempt by user {user?.FullName} with ID {user?.Id} to delete review ({id})");
                return Unauthorized("You are not authorized to delete this review");
            }
            try
            {
                var review = await _unitOfWork.Reviews.GetByIdAsync(id);

                if (review == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", review.Id.ToString(), "Review", $"There is no review", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    return BadRequest("There is no review");
                }
                await _unitOfWork.Reviews.Remove(review);
                _cache.Remove(_reviewsCacheKey);
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", review.Id.ToString(), "Review", $"Delete review ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                return Ok("Review Deleted: \n" + review);
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Delete", id.ToString(), "Review", $"Error deleting review", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error deleting review ({id})");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetReviewsByCourse(int courseId)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            try
            {
                var reviews = await _unitOfWork.Reviews.GetReviewsByCourseAsync(courseId);
                if (reviews == null || !reviews.Any())
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetReviewsByInstructor", courseId.ToString(), "Review", $"No reviews found for instructor ID: {courseId}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    return NotFound("No reviews found for this instructor");
                }
                await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetReviewsByInstructor", courseId.ToString(), "Review", $"Found {reviews.Count()} reviews for instructor ID: {courseId}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                return Ok(reviews);
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "GetReviewsByInstructor", courseId.ToString(), "Review", $"Error getting reviews for instructor ID: {courseId}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogError(ex, $"Error getting reviews for instructor ID: {courseId}");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("course/{courseId}student/{studentId}")]
        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> GetReviewsByCourseAndStudent(int courseId, int studentId)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            if (user.Id == studentId || user.UserRole.Name == "Admin")
            {
                try
                {
                    var review = _unitOfWork.Reviews.GetReviewByStudentAndCourseAsync(studentId, courseId);
                    if (review == null)
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetReviewsByStudent", studentId.ToString(), "Review", $"No reviews found for student ID: {studentId}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                        return NotFound("No reviews found for this student");
                    }
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetReviewsByStudent", studentId.ToString(), "Review", $"Found review {review.Id} for student ID: {studentId}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    return Ok(review);
                }
                catch (Exception ex)
                {
                    await _loggingService.LogAsync("System", "System", "System", "GetReviewsByStudent", studentId.ToString(), "Review", $"Error getting reviews for student ID: {studentId}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogError(ex, $"Error getting reviews for student ID: {studentId}");
                    return StatusCode(500, "Internal server error");
                }
            }
            else
            {
                await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetReviewsByStudent", studentId.ToString(), "Review", $"Unauthorized access attempt to get reviews for student ID: {studentId}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogWarning($"Unauthorized access attempt by user {user?.FullName} with ID {user?.Id} to get reviews for student ID: {studentId}");
                return Unauthorized();
            }
        }
        [HttpGet("average-rating/{courseId}")]
        public async Task<IActionResult> GetAverageRatingForCourse(int courseId)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            try
            {
                var averageRating = await _unitOfWork.Reviews.GetAverageRatingForCourseAsync(courseId);
                if (averageRating < 0)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetAverageRatingForCourse", courseId.ToString(), "Review", $"No reviews found for course ID: {courseId}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    return NotFound("No reviews found for this course");
                }
                await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetAverageRatingForCourse", courseId.ToString(), "Review", $"Average rating for course ID: {courseId} is {averageRating}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                return Ok(averageRating);
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "GetAverageRatingForCourse", courseId.ToString(), "Review", $"Error getting average rating for course ID: {courseId}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogError(ex, $"Error getting average rating for course ID: {courseId}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
