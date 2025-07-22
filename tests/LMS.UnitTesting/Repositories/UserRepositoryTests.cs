using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Repo;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace tests.LMS.UnitTesting.Repositories
{
    // tests/MyApi.UnitTests/Repositories/UserRepositoryTests.cs
    
    public class UserRepositoryTests
    {
        private readonly Mock<ApplicationDbContext> _mockContext;
        private readonly UserRepository _repository;

        public UserRepositoryTests()
        {
            _mockContext = new Mock<ApplicationDbContext>();
            _repository = new UserRepository(_mockContext.Object);
        }

        [Fact]
        public async Task GetByIdAsync_UserExists_ReturnsUser()
        {
            // Arrange
            var testUser = new User { Id = 10, FirstName = "Test", LastName="User", Email="user@example.com",Password="Password8", PhoneNumber="01111111111", Orders = [], Enrollments = [], Courses = [] };
            var mockSet = new Mock<DbSet<User>>();

            mockSet.Setup(m => m.FindAsync(1)).ReturnsAsync(testUser);
            _mockContext.Setup(c => c.Users).Returns(mockSet.Object);

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Id);
            Assert.Equal("Test User", result.FullName);
            Assert.Equal("user@example.com", result.Email);
            Assert.Equal(false, result.Courses.Any());
            Assert.Equal(false, result.Enrollments.Any());
            Assert.Equal(false, result.Orders.Any());
        }

        [Fact]
        public async Task AddAsync_ValidUser_ReturnsAddedUser()
        {
            // Arrange
            var newUser = new User { FirstName = "Test", LastName = "User", Email = "user@example.com", Password = "Password8", PhoneNumber = "01111111111" };
            var mockSet = new Mock<DbSet<User>>();

            _mockContext.Setup(c => c.Users).Returns(mockSet.Object);
            _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                      .ReturnsAsync(1);

            // Act
            var result = await _repository.AddAsync(newUser);

            // Assert
            mockSet.Verify(m => m.AddAsync(newUser, It.IsAny<CancellationToken>()), Times.Once());
            _mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
            Assert.Equal("New User", result.FullName);
            Assert.Equal("user@example.com", result.Email);
            Assert.Equal(false, result.Courses.Any());
            Assert.Equal(false, result.Enrollments.Any());
            Assert.Equal(false, result.Orders.Any());
        }
    }
}
