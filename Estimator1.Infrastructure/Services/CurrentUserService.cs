using Estimator1.Core.Enums;
using Estimator1.Core.Interfaces;
using Estimator1.Core.Models;

namespace Estimator1.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public event EventHandler<User?>? UserChanged;
        private User? _currentUser;
        private readonly ILoggingService _loggingService;

        public CurrentUserService(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public User? CurrentUser => _currentUser;
        public bool IsLoggedIn => _currentUser != null;
        public AccessLevel CurrentAccessLevel => _currentUser?.AccessLevel ?? AccessLevel.Basic;

        public bool HasRequiredAccess(AccessLevel requiredLevel)
        {
            _loggingService.Info($"[CurrentUserService] HasRequiredAccess called for level: {requiredLevel}");
            
            if (_currentUser == null)
            {
                _loggingService.Warn("[CurrentUserService] Access check failed: No user logged in");
                return false;
            }

            _loggingService.Info($"[CurrentUserService] Checking access for user: {_currentUser.Username}, Level: {_currentUser.AccessLevel}");
            bool hasAccess = false;

            switch (_currentUser.AccessLevel)
            {
                case AccessLevel.Administrator:
                    // Admin has access to everything
                    _loggingService.Info("[CurrentUserService] User is Administrator, granting access");
                    hasAccess = true;
                    break;
                case AccessLevel.Supervisor:
                    // Supervisor has access to Basic and Supervisor
                    hasAccess = requiredLevel <= AccessLevel.Supervisor;
                    _loggingService.Info($"[CurrentUserService] User is Supervisor, access granted: {hasAccess}");
                    break;
                case AccessLevel.Basic:
                    // Basic only has access to Basic
                    hasAccess = requiredLevel == AccessLevel.Basic;
                    _loggingService.Info($"[CurrentUserService] User is Basic, access granted: {hasAccess}");
                    break;
            }

            if (!hasAccess)
            {
                _loggingService.Warn($"[CurrentUserService] Access check failed: User {_currentUser.Username} with level {_currentUser.AccessLevel} attempted to access feature requiring {requiredLevel}");
            }
            else
            {
                _loggingService.Info($"[CurrentUserService] Access granted for user {_currentUser.Username} to access level {requiredLevel}");
            }

            return hasAccess;
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
            _loggingService.Info($"User set: {user.Username} with access level {user.AccessLevel}");
            UserChanged?.Invoke(this, user);
        }

        public void ClearCurrentUser()
        {
            if (_currentUser != null)
            {
                _loggingService.Info($"User cleared: {_currentUser.Username}");
                _currentUser = null;
                UserChanged?.Invoke(this, null);
            }
        }
    }
}
