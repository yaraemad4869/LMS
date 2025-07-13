using LearningManagementSystem.IServices;
using LearningManagementSystem.IUOW;
using LearningManagementSystem.Models;
using LearningManagementSystem.Services;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LearningManagementSystem.Services
{
    public class UserService : IUserService
    {
        private readonly RedisCacheService _redisListService;
        private readonly IUnitOfWork _unitOfWork;
        public UserService(IUnitOfWork unitOfWork, RedisCacheService redisListService)
        {
            _unitOfWork = unitOfWork;
            _redisListService = redisListService;
        }
        public async Task<User> GetUserByIdAsync(int id)
        {
            //const string cacheKey = "UserById";

            //// Try to get data from cache
            //var cachedDataStr = await _redisListService.;
            //User cachedData = new User();
            //if (cachedDataStr == null)
            //{
               User cachedData = await _unitOfWork.Users.GetByIdAsync(id);
            //    var cacheOptions = new DistributedCacheEntryOptions
            //    {
            //        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
            //        SlidingExpiration = TimeSpan.FromMinutes(10)
            //    };

            //    // Save data in cache
            //    await _cache.SetStringAsync(cacheKey, cachedData, cacheOptions);
            //}
            //cachedData = JsonSerializer.Deserialize<List<User>>(cachedDataStr);
            return cachedData;
        }
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _unitOfWork.Users.GetByEmailAsync(email);
        }
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _unitOfWork.Users.GetByUsernameAsync(username);
        }
        public async Task<IEnumerable<User>> GetAllInstructorsAsync()
        {
            return await _unitOfWork.Users.GetInstructorsAsync();
        }
        public async Task<IEnumerable<User>> GetAllStudentsAsync()
        {
            return await _unitOfWork.Users.GetStudentsAsync();
        }
        public async Task<User> CreateUserAsync(User user, string password)
        {
            if (await GetUserByEmailAsync(user.Email) != null)
            {
                throw new ApplicationException("Email already in use");
            }

            if (await GetUserByUsernameAsync(user.UserName) != null)
            {
                throw new ApplicationException("The username is already in use");
            }

            user.Password = HashPassword(password);
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            return user;
        }
        public async Task UpdateUserAsync(User user)
        {
            var existingUser = await GetUserByIdAsync(user.Id);
            if (existingUser == null)
            {
                throw new ApplicationException("User not found");
            }

            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.IsActive = user.IsActive;

            _unitOfWork.Users.Update(existingUser);
            await _unitOfWork.CompleteAsync();
        }
        public async Task DeleteUserAsync(int id)
        {
            var user = await GetUserByIdAsync(id);
            if (user == null)
            {
                throw new ApplicationException("User not found");
            }

            user.IsActive = false;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();
        }
        public async Task<bool> ValidatePasswordAsync(string username, string password)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null)
            {
                return false;
            }

            return VerifyPassword(password, user.Password);
        }
        public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new ApplicationException("User not found");
            }

            if (!VerifyPassword(currentPassword, user.Password))
            {
                throw new ApplicationException("The current password is incorrect.");
            }

            user.Password = HashPassword(newPassword);
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();
        }
        public async Task<User> AuthenticateAsync(string usernameOrEmail, string password)
        {
            User user = await _unitOfWork.Users.GetByUsernameAsync(usernameOrEmail);
            if (user == null)
            {
                user = await _unitOfWork.Users.GetByEmailAsync(usernameOrEmail);
            }

            if (user == null || !user.IsActive)
            {
                return null;
            }

            if (!VerifyPassword(password, user.Password))
            {
                return null;
            }

            user.LastLogin = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            return user;
        }
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }
    }
}
