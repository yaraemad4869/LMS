using LearningManagementSystem.IUOW;
using LearningManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LearningManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IUnitOfWork _unitOfWork;
        public OrdersController(IHttpContextAccessor contextAccessor,IUnitOfWork unitOfWork)
        {
            _contextAccessor = contextAccessor;
            _unitOfWork = unitOfWork;

        }
        [HttpGet]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetAll()
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User.Identity.Name);
            return Ok(_unitOfWork.Orders.GetOrders(user));

        }
        [HttpGet("cart")]
        [Authorize(Roles= "Student")]
        public async Task<IActionResult> GetCart()
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User.Identity.Name);
            return Ok(_unitOfWork.Orders.GetCart(user));

        }

        [HttpPost("add-to-cart/{courseId}")]
        [Authorize(Roles ="Student")]
        public async Task<IActionResult> AddToCart(int courseId)
        {
            User? user = await _unitOfWork.Users.GetByEmailAsync(User.Identity.Name);
            Course? course = await _unitOfWork.Courses.GetByIdAsync(courseId);
            if (course == null)
                return NotFound();
            Order? cart= await _unitOfWork.Orders.AddToCart(course,user);
            return Ok(cart);
        }
    }
}
