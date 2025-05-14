using Estimator1.Core.Enums;
using Estimator1.Core.Interfaces;
using Estimator1.Core.Models;

namespace Estimator1.Infrastructure.Services
{
    public class MockAuthenticationService : IAuthenticationService
    {
        private readonly ILoggingService _loggingService;
        private readonly Dictionary<string, int> _failedAttempts = new();
        private readonly Dictionary<string, DateTime?> _lockoutEnds = new();

        public MockAuthenticationService(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public async Task<(bool success, User? user, string message)> LoginAsync(string username, string password)
        {
            await Task.Delay(100); // Small delay

            // For testing, accept any username with password "test123"
            if (password == "test123")
            {
                // Assign access level based on username
                var accessLevel = username.ToLower() switch
                {
                    "admin" => AccessLevel.Administrator,
                    "supervisor" => AccessLevel.Supervisor,
                    _ => AccessLevel.Basic
                };

                var user = new User
                {
                    Id = 1,
                    Username = username,
                    AccessLevel = accessLevel,
                    Email = $"{username}@test.com",
                    PasswordHash = "test123" // Simple mock hash
                };

                _loggingService.Info($"Mock login successful for user: {username}");
                return (true, user, "Login successful");
            }

            _loggingService.Warn($"Mock login failed for user: {username}");
            return (false, null, "Invalid username or password");
        }

        public Task<bool> ValidatePasswordAsync(string password)
        {
            return Task.FromResult(password.Length >= 6);
        }

        public Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            return Task.FromResult(true);
        }

        public Task<DateTime?> UpdateLastLoginAsync(int userId)
        {
            return Task.FromResult<DateTime?>(DateTime.UtcNow);
        }
    }
}
