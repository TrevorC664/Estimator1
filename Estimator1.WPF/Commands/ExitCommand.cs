using System;
using System.Windows;
using System.Windows.Input;
using Estimator1.Core.Interfaces;

namespace Estimator1.WPF.Commands
{
    public class ExitCommand : ICommand
    {
        private readonly ILoggingService _loggingService;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public ExitCommand(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public bool CanExecute(object? parameter)
        {
            return true; // Exit is always available
        }

        public void Execute(object? parameter)
        {
            _loggingService.Info("[ExitCommand] Shutting down application");
            Application.Current.Shutdown();
        }
    }
}
