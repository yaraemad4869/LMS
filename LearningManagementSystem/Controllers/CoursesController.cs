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
    public class CoursesController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CoursesController> _logger;
        private readonly ILoggingService _loggingService;
        private readonly IMemoryCache _cache;
        private readonly string _coursesCacheKey = "CoursesCache";
        private readonly string _courseIdCacheKey = "CourseIdCache";

        public CoursesController(IMapper mapper,IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ILoggingService loggingService, ILogger<CoursesController> logger, UserManager<User> userManager, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
            _cache = cache;
            _loggingService = loggingService;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            //_logger.LogInformation($"{user.FullName} with id {user.Id} is getting all courses");
            try
            {
                if (_cache.TryGetValue(_coursesCacheKey, out IEnumerable<Course> cachedCourses))
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetCourses", "", "Course", $"Retrieved {cachedCourses.Count()} courses from cache", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting all courses from cache");

                    return Ok(_mapper.Map<CourseDto>(cachedCourses));
                }
                else
                {
                    var courses = await _unitOfWork.Courses.GetAllAsync();
                    if(courses != null || courses.Any()){
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5)) // Cache expires if not accessed for 5 mins
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))  // Cache expires after 1 hour
                    .SetPriority(CacheItemPriority.Normal);

                    // Save data in cache
                    _cache.Set(_coursesCacheKey, courses, cacheEntryOptions);

                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetCourses", "", "Course", $"Retrieved {courses.Count()} courses from database", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting all courses from database");

                        return Ok(_mapper.Map<IEnumerable<CourseDto>>(courses));
                    }
                    return Ok(null);
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "GetCourses", "", "Course", $"Error getting courses", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, "Error getting courses");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);

            try
            {
                if (_cache.TryGetValue(_courseIdCacheKey, out Course cachedCourse))
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetById", cachedCourse.Id.ToString(), "Course", $"Retrieved course ({cachedCourse.Id}) from cache", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting course {id} from cache");

                    return Ok(_mapper.Map<CourseDto>(cachedCourse));
                }
                else
                {
                    var course = await _unitOfWork.Courses.GetByIdAsync(id);
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5)) // Cache expires if not accessed for 5 mins
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))  // Cache expires after 1 hour
                .SetPriority(CacheItemPriority.Normal);

                    // Save data in cache
                    _cache.Set(_courseIdCacheKey, course, cacheEntryOptions);
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetById", cachedCourse.Id.ToString(), "Course", $"Retrieved course ({cachedCourse.Id}) from database", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting course {id} from database");

                    return Ok(_mapper.Map<CourseDto>(course));
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "GetById", id.ToString(), "Course", $"Error getting course ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error getting course {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize(Roles ="Instructor")]
        public async Task<IActionResult> Post(CourseDto courseDto)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            try
            {
                if (courseDto == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Post", "", "Course", $"There is no course", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.Id} with role {user?.UserRole.Name} posts without given course .");

                    return BadRequest("There is no course");
                }
                Course course = _mapper.Map<Course>(courseDto);
                course.InstructorId = user.Id;
                await _unitOfWork.Courses.AddAsync(course);
                _cache.Remove(_coursesCacheKey);
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Post", "", "Course", $"Posted new course", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogInformation($"{user.FullName} with id {user.Id} is posting a new course");

                return CreatedAtAction(nameof(Post), courseDto);
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Post", "", "Course", $"Error posting new course", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error posting course");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Put(int id, Course course)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            try
            {
                if (course == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", course.Id.ToString(), "Course", $"There is no course", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.Id} with role {user?.UserRole.Name} updates without given course.");

                    return BadRequest("There is no course");
                }
                else if (id != course.Id)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", course.Id.ToString(), "Course", $"Course ID mismatch", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.Id} with role {user?.UserRole.Name} updates wrong course.");

                    return BadRequest("Course ID mismatch");
                }
                var existingCourse = await _unitOfWork.Courses.GetByIdAsync(id);
                if (existingCourse == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", course.Id.ToString(), "Course", $"Course not found", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.Id} with role {user?.UserRole.Name} updates non-existing enrollment .");

                    return NotFound("Course not found");
                }
                await _unitOfWork.Courses.Update(course);
                _cache.Remove(_coursesCacheKey);
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", course.Id.ToString(), "Course", $"Put course ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogInformation($"{user.FullName} with id {user.Id} updates course {id}");

                return Ok(course);
            }

            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Put", course.Id.ToString(), "Course", $"Error putting course", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error putting course ({course.Id})");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> Delete(int id)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            try
            {
                var course = await _unitOfWork.Courses.GetByIdAsync(id);

                if (course == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", course.Id.ToString(), "Course", $"There is no course", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.Id} with role {user?.UserRole.Name} deletes non-existing course.");

                    return BadRequest("There is no course");
                }
                await _unitOfWork.Courses.Remove(course);
                _cache.Remove(_coursesCacheKey);
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", course.Id.ToString(), "Course", $"Delete course ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogInformation($"{user.FullName} with id {user.Id} is deleting course {id}");

                return Ok("Course Deleted: \n" + _mapper.Map<CourseDto>(course));
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Delete", id.ToString(), "Course", $"Error deleting course", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error deleting course ({id})");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("search")]
        public async Task<IActionResult> Search(string query)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            try
            {
                var courses = await _unitOfWork.Courses.SearchCoursesAsync(query);
                if (courses == null || !courses.Any())
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "Search", query, "Course", $"No courses found for search query: {query}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is searching courses with {query} that are 0");

                    return NotFound("No courses found");
                }
                await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "Search", query, "Course", $"Found {courses.Count()} courses for search query: {query}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogInformation($"{user.FullName} with id {user.Id} is getting courses with {query}");

                return Ok(_mapper.Map<IEnumerable<CourseDto>>(courses));
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Search", query, "Course", $"Error searching courses with query: {query}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogError(ex, $"Error searching courses with query: {query}");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("instructor/{instructorId}")]
        public async Task<IActionResult> GetCoursesByInstructor(int instructorId)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            try
            {
                var courses = await _unitOfWork.Courses.GetCoursesByInstructorAsync(instructorId);
                if (courses == null || !courses.Any())
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetCoursesByInstructor", instructorId.ToString(), "Course", $"No courses found for instructor ID: {instructorId}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting courses of instructor {instructorId} that are 0");

                    return NotFound("No courses found for this instructor");
                }
                await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetCoursesByInstructor", instructorId.ToString(), "Course", $"Found {courses.Count()} courses for instructor ID: {instructorId}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogInformation($"{user.FullName} with id {user.Id} is getting courses of instructor {instructorId}");

                return Ok(_mapper.Map<IEnumerable<CourseDto>>(courses));
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "GetCoursesByInstructor", instructorId.ToString(), "Course", $"Error getting courses for instructor ID: {instructorId}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogError(ex, $"Error getting courses for instructor ID: {instructorId}");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Student")]
        public async Task<IActionResult> GetCoursesByStudent(int studentId)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            if (user.Id == studentId || user.UserRole.Name=="Admin")
            {
                try
                {
                    var courses = user.Courses.ToList();
                    if (courses == null || !courses.Any())
                    {
                        await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetCoursesByStudent", studentId.ToString(), "Course", $"No courses found for student ID: {studentId}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                        _logger.LogInformation($"{user.FullName} with id {user.Id} is getting courses of student {studentId} that are 0");

                        return NotFound("No courses found for this student");
                    }
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetCoursesByStudent", studentId.ToString(), "Course", $"Found {courses.Count()} courses for student ID: {studentId}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting courses of student {studentId}");
                    return Ok(_mapper.Map<IEnumerable<CourseDto>>(courses));
                }
                catch (Exception ex)
                {
                    await _loggingService.LogAsync("System", "System", "System", "GetCoursesByStudent", studentId.ToString(), "Course", $"Error getting courses for student ID: {studentId}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogError(ex, $"Error getting courses for student ID: {studentId}");
                    return StatusCode(500, "Internal server error");
                }
            }
            else
            {
                await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetCoursesByStudent", studentId.ToString(), "Course", $"Unauthorized access attempt to get courses for student ID: {studentId}", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogWarning($"Unauthorized access attempt by user {user?.FullName} with ID {user?.Id} to get courses for student ID: {studentId}");
                return Unauthorized();
            }
        }

        [HttpGet("popular/{count}")]
        public async Task<IActionResult> GetPopularCourses(int count)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            try
            {
                var courses = await _unitOfWork.Courses.GetPopularCoursesAsync(count);
                if (courses == null || !courses.Any())
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetPopularCourses", "", "Course", $"No popular courses found", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting {count} popular courses that are 0");

                    return NotFound("No popular courses found");
                }
                await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetPopularCourses", "", "Course", $"Found {courses.Count()} popular courses", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogInformation($"{user.FullName} with id {user.Id} is getting {count} popular courses");

                return Ok(_mapper.Map<IEnumerable<CourseDto>>(courses));
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "GetPopularCourses", "", "Course", $"Error getting popular courses", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogError(ex, $"Error getting {count} popular courses");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("recent/{count}")]
        public async Task<IActionResult> GetRecentCourses(int count)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            try
            {
                var courses = await _unitOfWork.Courses.GetNewestCoursesAsync(count);
                if (courses == null || !courses.Any())
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetRecentCourses", "", "Course", $"No recent courses found", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting {count} recent courses that are 0");
                    return NotFound("No recent courses found");
                }
                await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetRecentCourses", "", "Course", $"Found {courses.Count()} recent courses", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogInformation($"{user.FullName} with id {user.Id} is getting {count} recent courses");

                return Ok(_mapper.Map<IEnumerable<CourseDto>>(courses));
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "GetRecentCourses", "", "Course", $"Error getting recent courses", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogError(ex, $"Error getting {count} recent courses");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
