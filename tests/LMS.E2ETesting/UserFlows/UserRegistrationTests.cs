using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using tests.LMS.E2ETesting.TestHelpers;
using Xunit;
namespace tests.LMS.E2ETesting.UserFlows
{
    public class UserRegistrationTests : IClassFixture<TestApplicationFactory>, IDisposable
    {
        private readonly HttpClient _client;
        private readonly TestApplicationFactory _factory;
        private readonly Mock<ApplicationDbContext> _mockContext;

        public UserRegistrationTests(TestApplicationFactory factory,Mock<ApplicationDbContext> mockContext)
        {
            _factory = factory;
            _client = factory.CreateClient();
            _mockContext = mockContext;
        }

        [Fact]
        public async Task RegisterUser_ValidData_CreatesUser()
        {
            // Arrange
            var userData = new { FirstName = "Test", LastName = "User", Email = "user@example.com", Password = "Password8", PhoneNumber = "01111111111" };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", userData);

            // Assert
            response.EnsureSuccessStatusCode();
            // Add more assertions as needed
        }

        public async Task LoginUser_ValidData()
        {
            var testUser = new User { Id = 10, FirstName = "Test", LastName = "User", Email = "user@example.com", Password = "Password8", PhoneNumber = "01111111111", Orders = [], Enrollments = [], Courses = [] };
            var mockSet = new Mock<DbSet<User>>();

            mockSet.Setup(m => m.FindAsync(1)).ReturnsAsync(testUser);
            _mockContext.Setup(c => c.Users).Returns(mockSet.Object);

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", testUser);

            // Assert
            response.EnsureSuccessStatusCode();
            // Add more assertions as needed
        }

        public void Dispose()
        {
            // Clean up test data
            _client.Dispose();
        }
    }
}
