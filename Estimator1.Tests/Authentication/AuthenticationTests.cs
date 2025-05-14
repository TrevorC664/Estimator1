using System;
using System.Threading.Tasks;
using BC = BCrypt.Net.BCrypt;
using Estimator1.Infrastructure.Services;
using Estimator1.Core.Interfaces;
using Estimator1.Core.Models;
using Estimator1.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Estimator1.Tests.Authentication
{
    public class AuthenticationTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuthenticationService _authService;
        
        public AuthenticationTests()
        {
            // Create a new in-memory database for each test
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
                
            _context = new ApplicationDbContext(options);
            _authService = new AuthenticationService(_context, new TestLoggingService());
        }

        [Fact]
        public async Task LoginAsync_WithInvalidCredentials_ShouldIncrementFailedAttempts()
        {
            // Arrange
            var username = "testuser";
            var password = "wrongpassword";
            var user = new User
            {
                Username = username,
                Email = "test@test.com",
                PasswordHash = BC.HashPassword("correctpassword"),
                FailedLoginAttempts = 0
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _authService.LoginAsync(username, password);
            
            // Assert
            Assert.False(result.success);
            user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            Assert.NotNull(user);
            Assert.Equal(1, user.FailedLoginAttempts);
        }

        [Fact]
        public async Task LoginAsync_ExceedMaxAttempts_ShouldLockAccount()
        {
            // Arrange
            var username = "testuser";
            var password = "wrongpassword";
            const int maxAttempts = 5;
            var user = new User
            {
                Username = username,
                Email = "test@test.com",
                PasswordHash = BC.HashPassword("correctpassword"),
                FailedLoginAttempts = 0
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            
            // Act
            for (int i = 0; i < maxAttempts; i++)
            {
                await _authService.LoginAsync(username, password);
            }
            
            // Assert
            user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            Assert.NotNull(user);
            Assert.NotNull(user.LockoutEnd);
            Assert.True(user.LockoutEnd > DateTime.UtcNow);
        }

        [Fact]
        public async Task LoginAsync_WithLockedAccount_ShouldReturnError()
        {
            // Arrange
            var username = "lockeduser";
            var password = "testpassword";
            var user = new User
            {
                Username = username,
                Email = "locked@test.com",
                PasswordHash = "hashedpassword",
                LockoutEnd = DateTime.UtcNow.AddHours(1)
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _authService.LoginAsync(username, password);
            
            // Assert
            Assert.False(result.success);
            Assert.Contains("account is locked", result.message.ToLower());
        }

        [Fact]
        public async Task LoginAsync_SuccessfulLogin_ShouldResetFailedAttempts()
        {
            // Arrange
            var username = "testuser";
            var password = "correctpassword";
            var user = new User
            {
                Username = username,
                Email = "test@test.com",
                PasswordHash = BC.HashPassword(password),
                FailedLoginAttempts = 2
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            
            // Act
            var result = await _authService.LoginAsync(username, password);
            
            // Assert
            Assert.True(result.success);
            user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            Assert.NotNull(user);
            Assert.Equal(0, user.FailedLoginAttempts);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
