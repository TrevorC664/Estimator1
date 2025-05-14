using System;
using System.Windows;
using System.Windows.Input;
using Estimator1.Core.Enums;
using Estimator1.Core.Interfaces;
using Estimator1.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Estimator1.WPF.Commands
{
    public class LoginCommand : ICommand
    {
        private readonly LoginWindowViewModel _viewModel;
        private readonly IAuthenticationService _authService;
        private readonly ILoggingService _loggingService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IServiceProvider _serviceProvider;

        public LoginCommand(LoginWindowViewModel viewModel, IAuthenticationService authService, 
            ILoggingService loggingService, ICurrentUserService currentUserService,
            IServiceProvider serviceProvider)
        {
            _viewModel = viewModel;
            _authService = authService;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _serviceProvider = serviceProvider;
        }

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(_viewModel.Username) && 
                   !string.IsNullOrWhiteSpace(_viewModel.Password);
        }

        public async void Execute(object? parameter)
        {
            try
            {
                _viewModel.ErrorMessage = string.Empty;
                var result = await _authService.LoginAsync(_viewModel.Username, _viewModel.Password);

                if (result.success && result.user != null)
                {
                    _loggingService.Info($"[Auth] User logged in successfully - {_viewModel.Username}");

                    // Set current user
                    _currentUserService.SetCurrentUser(result.user);
                    _loggingService.Info($"[LoginCommand] Set current user: {result.user.Username}, Access Level: {result.user.AccessLevel}");

                    // Get current LoginWindow
                    var loginWindow = Application.Current.MainWindow as Views.LoginWindow;
                    
                    // Create and configure MainWindow
                    var mainWindow = _serviceProvider.GetRequiredService<Views.MainWindow>();
                    _loggingService.Info("[LoginCommand] MainWindow created");
                    
                    // Set MainWindow as the new main window before showing it
                    Application.Current.MainWindow = mainWindow;
                    
                    // Show MainWindow and ensure it's visible
                    mainWindow.Show();
                    mainWindow.WindowState = WindowState.Normal;
                    mainWindow.Activate();
                    mainWindow.Focus();
                    
                    // Hide and close LoginWindow
                    if (loginWindow != null)
                    {
                        loginWindow.Hide();
                        loginWindow.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                _viewModel.ErrorMessage = "An error occurred during login. Please try again.";
                _loggingService.Error(ex, $"[Auth] Login error: {ex.Message}");
                _loggingService.Debug($"[Auth] Stack trace: {ex.StackTrace}");
            }
        }
    }
}
