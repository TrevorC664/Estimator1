using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Estimator1.Core.Interfaces;
using Estimator1.Core.Models;
using Estimator1.Infrastructure.Data;

namespace Estimator1.Infrastructure.Services
{
    public class DatabaseAuthenticationService : IAuthenticationService
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher _passwordHasher;
        private readonly ILoggingService _loggingService;

        public DatabaseAuthenticationService(
            ApplicationDbContext context,
            ILoggingService loggingService)
        {
            _context = context;
            _passwordHasher = new PasswordHasher();
            _loggingService = loggingService;

            // Enable detailed database logging
            _context.Database.SetCommandTimeout(30); // 30 second timeout
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.TrackAll;
            _context.ChangeTracker.AutoDetectChangesEnabled = true;
            _context.ChangeTracker.LazyLoadingEnabled = false;
        }

        public async Task<(bool success, User? user, string message)> LoginAsync(string username, string password)
        {
            try
            {
                var user = await AuthenticateAsync(username, password);
                if (user != null)
                {
                    return (true, user, "Login successful");
                }
                return (false, null, "Invalid username or password");
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Login error: {ex.Message}");
                return (false, null, "An error occurred during login");
            }
        }

        private async Task<User?> AuthenticateAsync(string username, string password)
        {
            try
            {
                _loggingService.Debug($"Attempting database connection...");
                await _context.Database.CanConnectAsync();
                _loggingService.Debug($"Database connection successful.");

                _loggingService.Debug($"Searching for user with username: {username}");
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && u.IsActive);

                if (user == null)
                {
                    _loggingService.Debug($"User not found: {username}");
                    return null;
                }

                _loggingService.Debug($"Verifying password for user: {username}");
                if (!_passwordHasher.VerifyPassword(user.PasswordHash, password))
                {
                    _loggingService.Debug($"Invalid password for user: {username}");
                    return null;
                }

                _loggingService.Info($"User authenticated successfully - {username}");

                try
                {
                    // Update last login time
                    user.LastLoginAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    await UpdateLastLoginAsync(user.Id);
                }
                catch (Exception ex)
                {
                    // Log but don't fail login if we can't update the timestamp
                    _loggingService.Warn($"Could not update last login time: {ex.Message}");
                }

                return user;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Authentication error for user {username}: {ex.Message}");
                _loggingService.Error($"Stack trace: {ex.StackTrace}");
                throw new Exception($"Database error: {ex.Message}", ex);
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _loggingService.Warn($"Password change failed: User not found - ID: {userId}");
                    return false;
                }

                if (!_passwordHasher.VerifyPassword(user.PasswordHash, currentPassword))
                {
                    _loggingService.Warn($"Password change failed: Invalid current password - User: {user.Username}");
                    return false;
                }

                user.PasswordHash = _passwordHasher.HashPassword(newPassword);
                user.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _loggingService.Info($"Password changed successfully for user - {user.Username}");
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Password change error for user ID {userId}: {ex.Message}");
                throw;
            }
        }

        public async Task<DateTime?> UpdateLastLoginAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            _loggingService.Debug($"Updated last login time for user - {user.Username}");
            return user.LastLoginAt;
        }

        public Task<bool> ValidatePasswordAsync(string password)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
                return Task.FromResult(false);

            bool hasUpperCase = false;
            bool hasLowerCase = false;
            bool hasDigit = false;
            bool hasSpecialChar = false;

            foreach (char c in password)
            {
                if (char.IsUpper(c)) hasUpperCase = true;
                else if (char.IsLower(c)) hasLowerCase = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else if (!char.IsLetterOrDigit(c)) hasSpecialChar = true;
            }

            return Task.FromResult(hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar);
        }
    }
}
