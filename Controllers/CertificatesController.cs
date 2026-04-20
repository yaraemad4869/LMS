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
    public class CertificatesController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CertificatesController> _logger;
        private readonly IMapper _mapper;
        private readonly ILoggingService _loggingService;
        private readonly IMemoryCache _cache;
        private readonly string _certificatesCacheKey = "CertificatesCache";
        private readonly string _certificateIdCacheKey = "CertificateIdCache";

        public CertificatesController(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILoggingService loggingService, ILogger<CertificatesController> logger, UserManager<User> userManager, IMemoryCache cache)
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            if (user == null)
            {
                return Unauthorized("User not found");
            }
            //_logger.LogInformation($"{user.FullName} with id {user.Id} is getting all certificates");
            try
            {
                if (_cache.TryGetValue(_certificatesCacheKey, out IEnumerable<Certificate> cachedCertificates))
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetCertificates", "", "Certificate", $"Retrieved {cachedCertificates.Count()} certificates from cache", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} in role {user.UserRole.Name} is getting all certificates from cache");
                    return Ok(_mapper.Map<IEnumerable<CertificateDto>>(cachedCertificates));
                }
                else
                {
                    var certificates = await _unitOfWork.Certificates.GetAllAsync();
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5)) // Cache expires if not accessed for 5 mins
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))  // Cache expires after 1 hour
                .SetPriority(CacheItemPriority.Normal);

                    // Save data in cache
                    _cache.Set(_certificatesCacheKey, certificates, cacheEntryOptions);
                    

                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetCertificates", "", "Certificate", $"Retrieved {cachedCertificates.Count()} certificates from database", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} in role {user.UserRole.Name} is getting all certificates from database");
                    return Ok(_mapper.Map<IEnumerable<CertificateDto>>(certificates));
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "GetCertificates", "", "Certificate", $"Error getting certificates", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogError(ex, "Error getting certificates");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);

            try
            {
                if (_cache.TryGetValue(_certificateIdCacheKey, out Certificate cachedCertificate))
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "Unathorized", user?.UserRole.Name, "GetById", cachedCertificate.Id.ToString(), "Certificate", $"Retrieved certificate ({cachedCertificate.Id}) from cache", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} in role {user.UserRole.Name} is getting a certificate with id {id} from cache");
                    return Ok(_mapper.Map<IEnumerable<CertificateDto>>(cachedCertificate));
                }
                else
                {
                    var certificate = await _unitOfWork.Certificates.GetByIdAsync(id);
                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5)) // Cache expires if not accessed for 5 mins
                    .SetAbsoluteExpiration(TimeSpan.FromHours(1))  // Cache expires after 1 hour
                .SetPriority(CacheItemPriority.Normal);

                    // Save data in cache
                    _cache.Set(_certificateIdCacheKey, certificate, cacheEntryOptions);
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "Unathorized", user?.FullName ?? "System", user?.UserRole.Name, "GetById", cachedCertificate.Id.ToString(), "Certificate", $"Retrieved certificate ({cachedCertificate.Id}) from database", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} in role {user.UserRole.Name} is getting a certificate with id {id} from database");
                    return Ok(_mapper.Map<CertificateDto>(certificate));
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "GetById", id.ToString(), "Certificate", $"Error getting certificate ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error getting certificate {id}");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Put(int id, Certificate certificate)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            try
            {
                if (certificate == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", certificate.Id.ToString(), "Certificate", $"There is no certificate", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.FullName ?? "Unknown"} doesn't give certificate.");
                    return BadRequest("There is no certificate");
                }
                else if (id != certificate.Id)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", certificate.Id.ToString(), "Certificate", $"Certificate ID mismatch", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.FullName ?? "Unknown"} attempted to update certificate with id {id} that doesn't match with certificate given.");
                    return BadRequest("Certificate ID mismatch");
                }
                var existingCertificate = await _unitOfWork.Certificates.GetByIdAsync(id);
                if (existingCertificate == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", certificate.Id.ToString(), "Certificate", $"Certificate not found", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.FullName ?? "Unknown"} attempted to update certificate with id {id} that doesn't exist.");
                    return NotFound("Certificate not found");
                }
                await _unitOfWork.Certificates.Update(certificate);
                _cache.Remove(_certificatesCacheKey);
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Put", certificate.Id.ToString(), "Certificate", $"Put certificate ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogInformation($"{user.FullName} with id {user.Id} in role {user.UserRole.Name} updates a certificate with id {id}");
                return Ok(_mapper.Map<CertificateDto>(certificate));
            }

            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Put", certificate.Id.ToString(), "Certificate", $"Error putting certificate", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error putting certificate ({certificate.Id})");
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
                var certificate = await _unitOfWork.Certificates.GetByIdAsync(id);

                if (certificate == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", certificate.Id.ToString(), "Certificate", $"There is no certificate", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.FullName ?? "Unknown"} attempted to delete certificate with id {id} that doesn't exist.");
                    return BadRequest("There is no certificate");
                }
                await _unitOfWork.Certificates.Remove(certificate);
                _cache.Remove(_certificatesCacheKey);
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "Delete", certificate.Id.ToString(), "Certificate", $"Delete certificate ({id})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogInformation($"{user.FullName} with id {user.Id} in role {user.UserRole.Name} is deleting a certificate with id {id} from database");
                return Ok(_mapper.Map<CertificateDto>(certificate));
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Delete", id.ToString(), "Certificate", $"Error deleting certificate", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error deleting certificate ({id})");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("generate-certificate/{enrollmentId}")]
        [Authorize]
        public async Task<IActionResult> GenerateCertificatePdf(int enrollmentId)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            var enrollment = await _unitOfWork.Enrollments.GetByIdAsync(enrollmentId);
            if (user.Id != enrollment.StudentId && !User.IsInRole("Admin"))
            {
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GenerateCertificatePdf", enrollmentId.ToString(), "Certificate", $"Unauthorized access to generate certificate for enrollment ({enrollmentId})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogWarning($"User {user?.FullName ?? "Unknown"} attempted to generate certificate for enrollment {enrollmentId} without authorization.");
                return Unauthorized("You are not authorized to generate this certificate");
            }
            try
            {
                var certificate = await _unitOfWork.Certificates.GenerateCertificatePdf(enrollmentId);
                if (certificate == null)
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GenerateCertificate", $"Enrollment {enrollmentId}", "Certificate", $"Certificate could not be generated", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"Certificate could not be generated for enrollment {enrollmentId}.");
                    return NotFound("Certificate could not be generated");
                }
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5)) // Cache expires if not accessed for 5 mins
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))  // Cache expires after 1 hour
                .SetPriority(CacheItemPriority.Normal);

                // Save data in cache
                _cache.Set(_certificatesCacheKey, certificate, cacheEntryOptions);
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GenerateCertificate", $"Enrollment {enrollmentId}", "Certificate", $"Generate Certificate", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                return Ok(_mapper.Map<CertificateDto>(certificate));
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "Delete", "", "Certificate", $"Error generating certificate", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");

                _logger.LogError(ex, $"Error generating certificate");
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Student,Admin")]
        public async Task<IActionResult> GetCertificatesByStudent(int studentId)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User?.Identity?.Name);
            if(user.Id != studentId && !User.IsInRole("Admin"))
            {
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetCertificatesByStudent", studentId.ToString(), "Certificate", $"Unauthorized access to student ({studentId}) certificates", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogWarning($"User {user?.FullName ?? "Unknown"} attempted to access certificates for student {studentId} without authorization.");
                return Unauthorized("You are not authorized to view this student's certificates");
            }
            try
            {
                if (_cache.TryGetValue(_certificateIdCacheKey, out Certificate cachedCertificate))
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetCertificatesByStudent", studentId.ToString(), "Certificate", $"Retrieved certificates for student ({studentId}) from cache", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogInformation($"{user.FullName} with id {user.Id} in role {user.UserRole.Name} is getting all certificates of student {studentId} from cache");
                    return Ok(_mapper.Map<IEnumerable<CertificateDto>>(cachedCertificate));
                }
                var certificates = await _unitOfWork.Certificates.GetCertificatesByStudent(studentId);
                if (certificates == null || !certificates.Any())
                {
                    await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetCertificatesByStudent", studentId.ToString(), "Certificate", $"No certificates found for student ({studentId})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                    _logger.LogWarning($"User {user?.FullName ?? "Unknown"} is getting all certificates of student {studentId} that doesn't exist.");

                    return NotFound("No certificates found for this student");
                }
                await _loggingService.LogAsync(user?.Id.ToString() ?? "System", user?.FullName ?? "System", user?.UserRole.Name, "GetCertificatesByStudent", studentId.ToString(), "Certificate", $"Retrieved certificates for student ({studentId}) from database", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogInformation($"{user.FullName} with id {user.Id} in role {user.UserRole.Name} is getting all certificates of student {studentId} from database");

                return Ok(_mapper.Map<IEnumerable<CertificateDto>>(certificates));
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync("System", "System", "System", "GetCertificatesByStudent", studentId.ToString(), "Certificate", $"Error getting certificates for student ({studentId})", _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "");
                _logger.LogError(ex, $"Error getting certificates for student {studentId}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
