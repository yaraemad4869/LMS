using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using LearningManagementSystem.IServices;
using LearningManagementSystem.Models;
using LearningManagementSystem.Models.DTOs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
namespace LearningManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<User> _userManager;

        public AuthController(IAuthService authService, UserManager<User> userManager)
        {
            _authService = authService;
            _userManager = userManager;
        }
        private async Task SendEmailMessage(RegisterDto model)
        {
            var subject = "Welcome to LMS";
            var body = $"Hi, {model.FirstName} {model.LastName}\n"+
                       $"Welcome To Our Courses Platform !";

            var message = new MailMessage
            {
                From = new MailAddress("yara.emad486@gmail.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(model.Email);

            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Credentials = new NetworkCredential("yara.emad4869@gmail.com", "einp wvgz mgvc bisk");
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(message);
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if (await _authService.RegisterAsync(model))
            {
                await SendEmailMessage(model);

                return Ok(new { message = "User registered successfully" });
            }

            return BadRequest(new { message = "Registration failed" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var token = await _authService.LoginAsync(model);

            if (token != null)
            {
                return Ok(new { token = token });
            }

            return Unauthorized(new { message = "Invalid credentials" });
        }
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Ok("Logout Successfully");
        }
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                var user = await _authService.GetUserByIdAsync(userId);
                if (user != null)
                    return Ok(user);
            }

            return NotFound();
        }
        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("GoogleResponse")
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            // 1. Authenticate the external cookie
            var authenticateResult = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);

            if (!authenticateResult.Succeeded)
                return BadRequest(new { Error = "Error while authenticating with Google" });

            // 2. Extract claims from Google
            var email = authenticateResult.Principal.FindFirstValue(ClaimTypes.Email);
            var name = authenticateResult.Principal.FindFirstValue(ClaimTypes.Name);
            var googleId = authenticateResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var givenName = authenticateResult.Principal.FindFirstValue(ClaimTypes.GivenName);
            var surname = authenticateResult.Principal.FindFirstValue(ClaimTypes.Surname);

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(googleId))
            {
                return BadRequest(new { Error = "Google did not return required information" });
            }

            // 3. Find or create user in your system
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                // Create new user
                user = new User
                {
                    Email = email,
                    UserName = email,
                    FullName = name,
                    FirstName = givenName,
                    LastName = surname,
                    GoogleId = googleId,
                    EmailConfirmed = true // Google already verified the email
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    return BadRequest(new { Error = "Failed to create user", Details = createResult.Errors });
                }
            }
            else
            {
                // Update existing user's Google ID if not set
                if (string.IsNullOrEmpty(user.GoogleId))
                {
                    user.GoogleId = googleId;
                    await _userManager.UpdateAsync(user);
                }
            }

            // 4. Generate JWT token for API authentication
            var token = await _authService.GenerateTokenAsync(user);

            // 5. Clean up external cookie
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            // 6. Return token and basic user info
            return Ok(new
            {
                Token = token,
                User = new
                {
                    user.Id,
                    user.Email,
                    user.FullName,
                    user.FirstName,
                    user.LastName
                }
            });
        }

        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _authService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _authService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return Ok(user);
        }

        [HttpPost("assign-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignRole(AssignRoleDto model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId.ToString());
            if (user == null)
                return NotFound(new { message = "User not found" });

            var result = await _userManager.AddToRoleAsync(user, model.RoleName);
            if (result.Succeeded)
                return Ok(new { message = $"Role {model.RoleName} assigned to user {user.Email}" });

            return BadRequest(new { message = "Failed to assign role", errors = result.Errors });
        }

        [HttpPost("remove-role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveRole(AssignRoleDto model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId.ToString());
            if (user == null)
                return NotFound(new { message = "User not found" });

            var result = await _userManager.RemoveFromRoleAsync(user, model.RoleName);
            if (result.Succeeded)
                return Ok(new { message = $"Role {model.RoleName} removed from user {user.Email}" });

            return BadRequest(new { message = "Failed to remove role", errors = result.Errors });
        }

        [HttpDelete("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound();

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
                return Ok(new { message = "User deleted successfully" });

            return BadRequest(new { message = "Failed to delete user", errors = result.Errors });
        }
    }
}
