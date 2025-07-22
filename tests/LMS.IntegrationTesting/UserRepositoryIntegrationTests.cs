using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearningManagementSystem.Data;
using LearningManagementSystem.Models;
using LearningManagementSystem.Repo;
// tests/MyApi.IntegrationTests/Repositories/UserRepositoryIntegrationTests.cs
using Microsoft.EntityFrameworkCore;
using Xunit;
namespace tests.LMS.IntegrationTesting
{
    public class UserRepositoryIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UserRepository _repository;

        public UserRepositoryIntegrationTests()
        {
            // Use a fresh in-memory database for each test
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new UserRepository(_context);

            // Seed test data
            _context.Users.AddRange(
                new User { Id = 11, FirstName = "Test", LastName = "User", Email = "user11@example.com", Password = "Password8", PhoneNumber = "01111111110" },
                new User { Id = 12, FirstName = "Test", LastName = "User", Email = "user12@example.com", Password = "Password8", PhoneNumber = "01111111112" }
            );
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetByIdAsync_ExistingUser_ReturnsUser()
        {
            // Act
            var result = await _repository.GetByIdAsync(11);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(11, result.Id);
            Assert.Equal("Test", result.FirstName);
            Assert.Equal("user11@example.com", result.Email);
            Assert.Equal(false, result.Courses.Any());
            Assert.Equal(false, result.Enrollments.Any());
            Assert.Equal(false, result.Orders.Any());
        }

        [Fact]
        public async Task AddAsync_NewUser_AddsToDatabase()
        {
            // Arrange
            var newUser = new User { FirstName = "Test", LastName = "User", Email = "user13@example.com", Password = "Password8", PhoneNumber = "01111111113" };

            // Act
            var result = await _repository.AddAsync(newUser);
            var fromDb = await _context.Users.FindAsync(result.Id);

            // Assert
            Assert.NotNull(fromDb);
            Assert.Equal("Test User", fromDb.FullName);
            Assert.Equal(result.Id, fromDb.Id);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
