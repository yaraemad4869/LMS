using LearningManagementSystem.Models.DTOs;
using LearningManagementSystem.Models;

namespace LearningManagementSystem.IServices
{
    public interface IAuthService
    {
        Task<string> GenerateTokenAsync(User user);
        Task<bool> RegisterAsync(RegisterDto model);
        Task<string> LoginAsync(LoginDto model);
        Task<UserResponseDto> GetUserByIdAsync(int userId);
        Task<List<UserResponseDto>> GetAllUsersAsync();
    }
}
