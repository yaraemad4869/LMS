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
    public class InstructorsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<InstructorsController> _logger;
        private readonly IMapper _mapper;
        private readonly ILoggingService _loggingService;
        private readonly IMemoryCache _cache;
        private readonly string _instructorsCacheKey = "InstructorsCache";
        private readonly string _instructorIdCacheKey = "InstructorIdCache";

        public InstructorsController(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, ILoggingService loggingService, ILogger<InstructorsController> logger, UserManager<User> userManager, IMemoryCache cache)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
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
                _logger.LogWarning($"user {user?.FullName} with ID {user?.Id} attempts to get all instructors without permission");

                return Forbid("Not Allowed To See The Content");
            }
            //_logger.LogInformation($"{user.FullName} with id {user.Id} is getting all instructors");
            try
            {
                if (_cache.TryGetValue(_instructorsCacheKey, out IEnumerable<Instructor> cachedInstructors))
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetInstructors", "", "Instructor", $"Retrieved {cachedInstructors.Count()} instructors from cache", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting all instructors from cache");

                    return Ok(cachedInstructors);
                }
                else
                {
                    var instructors = await _unitOfWork.Instructors.GetAllAsync();
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5)) // Cache expires if not accessed for 5 mins
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))  // Cache expires after 1 hour
                .SetPriority(CacheItemPriority.Normal);

                    // Save data in cache
                    _cache.Set(_instructorsCacheKey, instructors, cacheEntryOptions);

                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetInstructors", "", "Instructor", $"Retrieved {cachedInstructors.Count()} instructors from database", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting all enrollments from database");

                    return Ok(instructors);
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "GetInstructors", "", "Instructor", $"Error getting instructors", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, "Error getting instructors");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);

            try
            {
                if (_cache.TryGetValue(_instructorIdCacheKey, out Instructor cachedInstructor))
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetById", cachedInstructor.Id.ToString(), "Instructor", $"Retrieved instructor ({cachedInstructor.Id}) from cache", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting instructor {id} from cache");

                    return Ok(_mapper.Map<UserDto>(cachedInstructor));
                }
                else
                {
                    var instructor = await _unitOfWork.Instructors.GetByIdAsync(id);
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5)) // Cache expires if not accessed for 5 mins
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))  // Cache expires after 1 hour
                .SetPriority(CacheItemPriority.Normal);

                    // Save data in cache
                    _cache.Set(_instructorIdCacheKey, instructor, cacheEntryOptions);
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetById", cachedInstructor.Id.ToString(), "Instructor", $"Retrieved instructor ({cachedInstructor.Id}) from database", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} is getting instructor {id} from database");

                    return Ok(_mapper.Map<UserDto>(instructor));
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "GetById", id.ToString(), "Instructor", $"Error getting instructor ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error getting instructor {id}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Post(Instructor instructor)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            try
            {
                if (instructor == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Post", instructor.Id.ToString(), "Instructor", $"There is no instructor", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                    return BadRequest("There is no instructor");
                }
                await _unitOfWork.Instructors.AddAsync(instructor);
                _cache.Remove(_instructorsCacheKey);
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Post", instructor.Id.ToString(), "Instructor", $"Posted new instructor", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                return CreatedAtAction(nameof(Post), new { id = instructor.Id }, _mapper.Map<UserDto>(instructor));
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Post", instructor.Id.ToString(), "Instructor", $"Error posting new instructor", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error posting instructor");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles ="Admin,Instructor")]
        public async Task<IActionResult> Put(int id, Instructor instructor)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            if(user.Id!=instructor.Id && User.IsInRole("Admin"))
            {
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", instructor.Id.ToString(), "Instructor", $"Unauthorized attempt to update instructor ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogWarning($"User {user?.FullName} with ID {user?.Id} attempted to update instructor {id} without authorization.");
                return Unauthorized("You are not authorized to update this instructor");
            }
            try
            {
                if (instructor == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", instructor.Id.ToString(), "Instructor", $"There is no instructor", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.FullName} with ID {user?.Id} attempted to update an instructor that does not exist.");
                    return BadRequest("There is no instructor");
                }
                else if (id != instructor.Id)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", instructor.Id.ToString(), "Instructor", $"Instructor ID mismatch", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.FullName} with ID {user?.Id} attempted to update instructor {id} with a different ID ({instructor.Id}).");
                    return BadRequest("Instructor ID mismatch");
                }
                var existingInstructor = await _unitOfWork.Instructors.GetByIdAsync(id);
                if (existingInstructor == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", instructor.Id.ToString(), "Instructor", $"Instructor not found", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.FullName} with ID {user?.Id} attempted to update instructor {id} that does not exist.");
                    return NotFound("Instructor not found");
                }
                await _unitOfWork.Instructors.Update(instructor);
                _cache.Remove(_instructorsCacheKey);
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", instructor.Id.ToString(), "Instructor", $"Put instructor ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogInformation($"User {user?.FullName} with ID {user?.Id} updated instructor {id} successfully.");
                return Ok(_mapper.Map<UserDto>(instructor));
            }

            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Put", instructor.Id.ToString(), "Instructor", $"Error putting instructor", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error putting instructor ({instructor.Id})");
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
                var instructor = await _unitOfWork.Instructors.GetByIdAsync(id);

                if (instructor == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", instructor.Id.ToString(), "Instructor", $"There is no instructor", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.Id} with role {user?.UserRole.Name} deletes instructor that does not exist.");

                    return BadRequest("There is no instructor");
                }
                await _unitOfWork.Instructors.Remove(instructor);
                _cache.Remove(_instructorsCacheKey);
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", instructor.Id.ToString(), "Instructor", $"Delete instructor ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogInformation($"{user.FullName} with id {user.Id} is deleting instructor {id}");

                return Ok("Instructor Deleted: \n" + _mapper.Map<UserDto>(instructor));
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Delete", id.ToString(), "Instructor", $"Error deleting instructor", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error deleting instructor ({id})");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
