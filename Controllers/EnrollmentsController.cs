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

        public EnrollmentsController(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ILoggingService loggingService, ILogger<EnrollmentsController> logger, UserManager<User> userManager, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
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
            try
            {
                if (_cache.TryGetValue(_enrollmentsCacheKey, out IEnumerable<Enrollment> cachedEnrollments))
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetEnrollments", "", "Enrollment", $"Retrieved {cachedEnrollments.Count()} enrollments from cache", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting all enrollments from cache");
                    return Ok(cachedEnrollments);
                }
                else
                {
                    var enrollments = await _unitOfWork.Enrollments.GetAllAsync();
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5)) // Cache expires if not accessed for 5 mins
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))  // Cache expires after 1 hour
                .SetPriority(CacheItemPriority.Normal);

                    _cache.Set(_enrollmentsCacheKey, enrollments, cacheEntryOptions);

                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetEnrollments", "", "Enrollment", $"Retrieved {cachedEnrollments.Count()} enrollments from database", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting all enrollments from database");

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
                    if(user?.Id != cachedEnrollment.StudentId && cachedEnrollment.Course.InstructorId != user.Id && !User.IsInRole("Admin"))
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetById", cachedEnrollment.Id.ToString(), "Enrollment", $"Is not allowed to retrieve enrollment ({cachedEnrollment.Id}) from cache", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                        _logger.LogWarning($"User {user?.Id} with role {user?.UserRole.Name} attempted to access enrollment {id} without permission.");
                        return Forbid("Not Allowed To See The Content");

                    }
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetById", cachedEnrollment.Id.ToString(), "Enrollment", $"Retrieved enrollment ({cachedEnrollment.Id}) from cache", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting enrollment {id} from cache");
                    return Ok(cachedEnrollment);
                }
                else
                {
                    var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(id);
                    if (enrollment.StudentId == user.Id || enrollment.Course.InstructorId == user.Id || user.UserRole.Name == "Admin")
                    {
                        var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromMinutes(5)) // Cache expires if not accessed for 5 mins
                        .SetAbsoluteExpiration(TimeSpan.FromHours(1))  // Cache expires after 1 hour
                    .SetPriority(CacheItemPriority.Normal);

                        // Save data in cache
                        _cache.Set(_enrollmentIdCacheKey, enrollment, cacheEntryOptions);
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetById", cachedEnrollment.Id.ToString(), "Enrollment", $"Retrieved enrollment ({cachedEnrollment.Id}) from database", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                        _logger.LogInformation($"{user.FullName} with id {user.Id} is getting enrollment {id} from database");

                        return Ok(enrollment);
                    }
                    else
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetById", cachedEnrollment.Id.ToString(), "Enrollment", $"Is not allowerd to retrieve enrollment ({cachedEnrollment.Id}) from database", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                        _logger.LogWarning($"User {user?.Id} with role {user?.UserRole.Name} attempted to access enrollment {id} without permission.");
                        return Forbid("Not Allowed To See The Content");
                    }
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
        [Authorize]
        public async Task<IActionResult> Post(Enrollment enrollment)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            try
            {
                if (enrollment == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Post", enrollment.Id.ToString(), "Enrollment", $"There is no enrollment", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.Id} with role {user?.UserRole.Name} posts without given enrollment .");
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
        [Authorize("Admin,Instructor")]
        public async Task<IActionResult> Put(int id, Enrollment enrollment)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            try
            {
                if (enrollment == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", enrollment.Id.ToString(), "Enrollment", $"There is no enrollment", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.Id} with role {user?.UserRole.Name} updates without given enrollment .");
                    return BadRequest("There is no enrollment");
                }
                else if (id != enrollment.Id)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", enrollment.Id.ToString(), "Enrollment", $"Enrollment ID mismatch", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.Id} with role {user?.UserRole.Name} attemps to update wrong enrollment .");

                    return BadRequest("Enrollment ID mismatch");
                }
                var existingEnrollment = await _unitOfWork.Enrollments.GetByIdAsync(id);
                if (existingEnrollment == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", enrollment.Id.ToString(), "Enrollment", $"Enrollment not found", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.Id} with role {user?.UserRole.Name} updates non-existing enrollment .");

                    return NotFound("Enrollment not found");
                }
                await _unitOfWork.Enrollments.Update(enrollment);
                _cache.Remove(_enrollmentsCacheKey);
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", enrollment.Id.ToString(), "Enrollment", $"Put enrollment ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogInformation($"{user.FullName} with id {user.Id} is updating enrollment {id}");
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
                    _logger.LogWarning($"User {user?.Id} with role {user?.UserRole.Name} deletes non-existing enrollment .");
                    return BadRequest("There is no enrollment");
                }
                await _unitOfWork.Enrollments.Remove(enrollment);
                _cache.Remove(_enrollmentsCacheKey);
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", enrollment.Id.ToString(), "Enrollment", $"Delete enrollment ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogInformation($"{user.FullName} with id {user.Id} is deleting enrollment {id}");

                return Ok("Enrollment Deleted: \n" + enrollment);
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Delete", id.ToString(), "Enrollment", $"Error deleting enrollment", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error deleting enrollment ({id})");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> GetEnrollmentsByStudentId(int studentId)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            if (user.Id == studentId || user.UserRole.Name == "Admin")
            {
                try
                {
                    var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsByStudentAsync(studentId);
                    if (enrollments == null || !enrollments.Any())
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetEnrollmentsByStudentId", studentId.ToString(), "Enrollment", $"No enrollments found for student ({studentId})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                        _logger.LogInformation($"{user.FullName} with id {user.Id} is getting enrollment of student {studentId} that are 0");

                        return NotFound("No enrollments found for this student");
                    }
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetEnrollmentsByStudentId", studentId.ToString(), "Enrollment", $"Retrieved enrollments for student ({studentId})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting enrollment of student {studentId}");

                    return Ok(enrollments);
                }
                catch (Exception ex)
                {
                    await _loggingService.LogAsync("System", "System", "System", "GetEnrollmentsByStudentId", studentId.ToString(), "Enrollment", $"Error getting enrollments for student ({studentId})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogError(ex, $"Error getting enrollments for student ({studentId})");
                    return StatusCode(500, "Internal server error");
                }
            }
            else
            {
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetEnrollmentsByStudentId", studentId.ToString(), "Enrollment", $"User {user?.Id} with role {user?.UserRole.Name} attempted to access enrollments for student ({studentId}) without permission", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogWarning($"User {user?.Id} with role {user?.UserRole.Name} attempted to access enrollments for student {studentId} without permission.");
                return Unauthorized("You do not have permission to view this student's enrollments");
            }
        }
        [HttpGet("course/{courseId}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetEnrollmentsByCourseAsync(int courseId)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            Course? course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null)
            {
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetEnrollmentsByCourseId", courseId.ToString(), "Enrollment", $"Course ({courseId}) not found", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogWarning($"User {user?.Id} with role {user?.UserRole.Name} get course that does not exist.");

                return NotFound("Course not found");
            }
            else if (course.InstructorId != user.Id && user.UserRole.Name != "Admin")
            {
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetEnrollmentsByCourseId", courseId.ToString(), "Enrollment", $"User {user?.Id} with role {user?.UserRole.Name} attempted to access enrollments for course ({courseId}) without permission", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogWarning($"User {user?.Id} with role {user?.UserRole.Name} attempted to access enrollments for course {courseId} without permission.");
                return Unauthorized("You do not have permission to view this course's enrollments");
            }
            try
            {
                var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsByCourseAsync(courseId);

                if (enrollments == null || !enrollments.Any())
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetEnrollmentsByCourseId", courseId.ToString(), "Enrollment", $"No enrollments found for course ({courseId})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting enrollment of course {courseId} that are 0");

                    return NotFound("No enrollments found for this course");
                }
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetEnrollmentsByCourseId", courseId.ToString(), "Enrollment", $"Retrieved enrollments for course ({courseId})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogInformation($"{user.FullName} with id {user.Id} is getting enrollment of course {courseId}");

                return Ok(enrollments);
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "GetEnrollmentsByCourseId", courseId.ToString(), "Enrollment", $"Error getting enrollments for course ({courseId})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogError(ex, $"Error getting enrollments for course ({courseId})");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("student/{studentId}/course/{courseId}")]
        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> GetEnrollment(int studentId, int courseId)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            if (user.Id == studentId || user.UserRole.Name == "Admin")
            {
                try
                {
                    var enrollment = await _unitOfWork.Enrollments.GetEnrollmentAsync(studentId, courseId);
                    if (enrollment == null)
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetEnrollmentByStudentAndCourse", $"{studentId}-{courseId}", "Enrollment", $"No enrollment found for student ({studentId}) in course ({courseId})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                        _logger.LogWarning($"User {user?.Id} with role {user?.UserRole.Name} get enrollment that does not exist.");

                        return NotFound("No enrollment found for this student in the specified course");
                    }
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetEnrollmentByStudentAndCourse", $"{studentId}-{courseId}", "Enrollment", $"Retrieved enrollment for student ({studentId}) in course ({courseId})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting enrollment of student {studentId}and course {courseId}");

                    return Ok(enrollment);
                }
                catch (Exception ex)
                {
                    await _loggingService.LogAsync("System", "System", "System", "GetEnrollmentByStudentAndCourse", $"{studentId}-{courseId}", "Enrollment", $"Error getting enrollment for student ({studentId}) in course ({courseId})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogError(ex, $"Error getting enrollment for student ({studentId}) in course ({courseId})");
                    return StatusCode(500, "Internal server error");
                }
            }
            else
            {
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetEnrollmentByStudentAndCourse", $"{studentId}-{courseId}", "Enrollment", $"User {user?.Id} with role {user?.UserRole.Name} attempted to access enrollment for student ({studentId}) in course ({courseId}) without permission", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogWarning($"User {user?.Id} with role {user?.UserRole.Name} attempted to access enrollment for student {studentId} in course {courseId} without permission.");
                return Unauthorized("You do not have permission to view this student's enrollment in the specified course");
            }
        }
    }
}
