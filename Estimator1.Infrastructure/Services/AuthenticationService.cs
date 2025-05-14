using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Estimator1.Core.Interfaces;
using Estimator1.Core.Models;
using Estimator1.Infrastructure.Data;
using BC = BCrypt.Net.BCrypt;

namespace Estimator1.Infrastructure.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILoggingService _loggingService;
        private const int MaxFailedAttempts = 5;
        private const int LockoutDurationMinutes = 30;

        public AuthenticationService(
            ApplicationDbContext context,
            ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public async Task<(bool success, User? user, string message)> LoginAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                await _loggingService.LogSecurityEventAsync(0, "LOGIN_FAILED", $"User not found: {username}");
                return (false, new User { 
                    Username = username, 
                    Email = "unknown@example.com", // Placeholder for non-existent user
                    PasswordHash = "not-a-real-hash", // Placeholder for non-existent user
                    FailedLoginAttempts = MaxFailedAttempts 
                }, "Invalid username or password");
            }

            // Check if account is locked
            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
            {
                var remainingLockoutTime = user.LockoutEnd.Value - DateTime.UtcNow;
                await _loggingService.LogSecurityEventAsync(
                    user.Id,
                    "LOGIN_BLOCKED",
                    $"Attempted login while locked out. Remaining lockout time: {remainingLockoutTime.TotalMinutes:F1} minutes"
                );
                return (false, null, $"Account is locked. Please try again in {remainingLockoutTime.TotalMinutes:F0} minutes");
            }

            // Verify password
            if (!BC.Verify(password, user.PasswordHash))
            {
                user.FailedLoginAttempts++;
                
                // Check if we should lock the account
                if (user.FailedLoginAttempts >= MaxFailedAttempts)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(LockoutDurationMinutes);
                    user.LockoutReason = "Maximum failed login attempts exceeded";
                    await _loggingService.LogSecurityEventAsync(
                        user.Id,
                        "ACCOUNT_LOCKED",
                        $"Account locked due to {MaxFailedAttempts} failed login attempts"
                    );
                }
                
                await _context.SaveChangesAsync();
                await _loggingService.LogSecurityEventAsync(
                    user.Id,
                    "LOGIN_FAILED",
                    $"Failed login attempt {user.FailedLoginAttempts} of {MaxFailedAttempts}"
                );
                
                return (false, null, "Invalid username or password");
            }

            // Successful login - reset failed attempts and update login time
            user.FailedLoginAttempts = 0;
            user.LockoutEnd = null;
            user.LockoutReason = null;
            user.LastLoginAt = DateTime.UtcNow;
            user.SecurityStamp = Guid.NewGuid().ToString(); // Invalidate old sessions
            
            await _context.SaveChangesAsync();
            await _loggingService.LogSecurityEventAsync(user.Id, "LOGIN_SUCCESS", "Successful login");
            
            return (true, user, "Login successful");
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

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Verify current password
            if (!BC.Verify(currentPassword, user.PasswordHash))
            {
                await _loggingService.LogSecurityEventAsync(
                    userId,
                    "PASSWORD_CHANGE_FAILED",
                    "Invalid current password"
                );
                return false;
            }

            // Validate new password
            if (!await ValidatePasswordAsync(newPassword))
            {
                await _loggingService.LogSecurityEventAsync(
                    userId,
                    "PASSWORD_CHANGE_FAILED",
                    "New password does not meet complexity requirements"
                );
                return false;
            }

            // Update password
            user.PasswordHash = BC.HashPassword(newPassword);
            user.SecurityStamp = Guid.NewGuid().ToString(); // Invalidate all sessions
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _loggingService.LogSecurityEventAsync(
                userId,
                "PASSWORD_CHANGED",
                "Password successfully changed"
            );

            return true;
        }

        public async Task<DateTime?> UpdateLastLoginAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return user.LastLoginAt;
        }
    }
}
