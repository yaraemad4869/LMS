using LearningManagementSystem.IServices;
using LearningManagementSystem.IUOW;
using LearningManagementSystem.Models;
using LearningManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace LearningManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : Controller
    {
        private readonly PayPalService _payPalService;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentController> _logger;
        private readonly ILoggingService _loggingService;
        public PaymentController(PayPalService payPalService, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork, ILogger<PaymentController> logger, ILoggingService loggingService)
        {
            _payPalService = payPalService;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _loggingService = loggingService;
        }
        [HttpPost("PaymentWithPayPal")]
        [Authorize]
        public async Task<IActionResult> PaymentWithPayPal()
        {
            User? user = await _userManager.GetUserAsync(User);
            await _loggingService.LogAsync(
                userId: user.Id.ToString(),
                userName: user.FullName,
                userRole: user.UserRole.Name,
                action: "PaymentWithPayPal",
                entityId: "",
                entityName: "Payment",
                details: "User initiated a payment with PayPal.",
                ipAddress: _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                userAgent: _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString()
            );
            _logger.LogInformation("User {UserId} initiated PayPal payment.", user.Id);
            var redirectUrl = "https://your-site.com/Payment/PaymentSuccess";
            var payment = _payPalService.CreatePayment(redirectUrl);

            var approvalUrl = payment.links.FirstOrDefault(l => l.rel.Equals("approval_url", System.StringComparison.OrdinalIgnoreCase)).href;
            if(approvalUrl == null)
            {
                return BadRequest("Unable to create PayPal payment.");
            }
            return Ok(approvalUrl);
        }
    }
}