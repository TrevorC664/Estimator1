using System.Windows.Input;
using System.Threading.Tasks;
using Estimator1.Core.Enums;
using Estimator1.Core.Interfaces;
using Estimator1.WPF.Commands;

namespace Estimator1.WPF.ViewModels
{
    public class AccessTestViewModel : ViewModelBase
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ILoggingService _loggingService;
        private string _accessTestResult = string.Empty;

        public AccessTestViewModel(ICurrentUserService currentUserService, ILoggingService loggingService)
        {
            _currentUserService = currentUserService;
            _loggingService = loggingService;

            TestBasicAccessCommand = new AsyncRelayCommand(TestBasicAccess);
            TestSupervisorAccessCommand = new AsyncRelayCommand(TestSupervisorAccess);
            TestAdminAccessCommand = new AsyncRelayCommand(TestAdminAccess);
        }

        public ICommand TestBasicAccessCommand { get; }
        public ICommand TestSupervisorAccessCommand { get; }
        public ICommand TestAdminAccessCommand { get; }

        public string AccessTestResult
        {
            get => _accessTestResult;
            set => SetField(ref _accessTestResult, value);
        }

        public string CurrentUserInfo
        {
            get
            {
                var user = _currentUserService.CurrentUser;
                return user != null
                    ? $"Logged in as: {user.Username} (Access Level: {user.AccessLevel})"
                    : "Not logged in";
            }
        }

        private async Task TestBasicAccess()
        {
            await TestAccess(AccessLevel.Basic);
        }

        private async Task TestSupervisorAccess()
        {
            await TestAccess(AccessLevel.Supervisor);
        }

        private async Task TestAdminAccess()
        {
            await TestAccess(AccessLevel.Administrator);
        }

        private async Task TestAccess(AccessLevel requiredLevel)
        {
            bool hasAccess = await Task.Run(() => _currentUserService.HasRequiredAccess(requiredLevel));
            string result = hasAccess
                ? $"Access GRANTED for {requiredLevel} level features"
                : $"Access DENIED for {requiredLevel} level features";

            await Task.Run(() => _loggingService.Info($"Access test for {requiredLevel}: {result}"));
            AccessTestResult = result;
        }
    }
}
