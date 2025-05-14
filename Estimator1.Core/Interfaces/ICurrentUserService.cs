using Estimator1.Core.Enums;
using Estimator1.Core.Models;

namespace Estimator1.Core.Interfaces
{
    public interface ICurrentUserService
    {
        event EventHandler<User?> UserChanged;
        User? CurrentUser { get; }
        bool IsLoggedIn { get; }
        AccessLevel CurrentAccessLevel { get; }
        bool HasRequiredAccess(AccessLevel requiredLevel);
        void SetCurrentUser(User user);
        void ClearCurrentUser();
    }
}
