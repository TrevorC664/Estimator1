using System;
using System.Windows.Input;
using Estimator1.Core.Enums;
using Estimator1.Core.Interfaces;

namespace Estimator1.WPF.Commands
{
    public class AdminCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILoggingService _loggingService;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public AdminCommand(Action<object?> execute, ICurrentUserService currentUserService, ILoggingService loggingService)
        {
            _execute = execute;
            _currentUserService = currentUserService;
            _loggingService = loggingService;
        }

        public bool CanExecute(object? parameter)
        {
            var hasAccess = _currentUserService.HasRequiredAccess(AccessLevel.Administrator);
            _loggingService.Info($"[AdminCommand] CanExecute called, hasAccess: {hasAccess}");
            return hasAccess;
        }

        public void Execute(object? parameter)
        {
            _loggingService.Info("[AdminCommand] Execute called");
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
