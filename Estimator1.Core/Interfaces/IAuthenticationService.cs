using Estimator1.Core.Models;

namespace Estimator1.Core.Interfaces
{
    public interface IAuthenticationService
    {
        Task<(bool success, User? user, string message)> LoginAsync(string username, string password);
        Task<bool> ValidatePasswordAsync(string password);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<DateTime?> UpdateLastLoginAsync(int userId);
    }
}
