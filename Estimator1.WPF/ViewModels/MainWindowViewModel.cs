using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Estimator1.Core.Enums;
using Estimator1.Core.Interfaces;
using Estimator1.Core.Models;
using Estimator1.WPF.Commands;
using Estimator1.WPF.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Estimator1.WPF.ViewModels
{
    /// <summary>
    /// ViewModel for the main window, handles user access control and navigation
    /// Implements INotifyPropertyChanged for UI updates and IDisposable for cleanup
    /// </summary>
    public class MainWindowViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly ICurrentUserService _currentUserService;
        private readonly ILoggingService _loggingService;
        private bool _isAdministrator;

        /// <summary>
        /// Indicates whether the current user has administrator access
        /// Updates UI and command availability when changed
        /// </summary>
        public bool IsAdministrator
        {
            get => _isAdministrator;
            private set
            {
                if (_isAdministrator != value)
                {
                    _isAdministrator = value;
                    OnPropertyChanged(nameof(IsAdministrator));
                    CommandManager.InvalidateRequerySuggested();  // Refresh command availability
                }
            }
        }

        // Commands exposed to the UI
        public ICommand ExitCommand { get; }  // Handles application shutdown
        public ICommand OpenSettingsCommand { get; }  // Opens User Management window (admin only)

        /// <summary>
        /// Initializes the ViewModel with required services and sets up commands
        /// </summary>
        public MainWindowViewModel(ICurrentUserService currentUserService, ILoggingService loggingService)
        {
            _currentUserService = currentUserService;
            _loggingService = loggingService;
            
            // Subscribe to user changes to update access rights
            _currentUserService.UserChanged += CurrentUserService_UserChanged;

            // Initialize commands
            ExitCommand = new ExitCommand(loggingService);
            OpenSettingsCommand = new AdminCommand(ExecuteOpenSettingsCommand, currentUserService, loggingService);

            _loggingService.Info("[MainWindowViewModel] Initializing view model");
            RefreshAccess();
        }

        /// <summary>
        /// Opens the User Management window as a modal dialog
        /// Only accessible to administrators
        /// </summary>
        private void ExecuteOpenSettingsCommand(object? obj)
        {
            try
            {
                var app = Application.Current as App ?? 
                    throw new InvalidOperationException("Application.Current is not of type App");

                _loggingService.Info("[MainWindowViewModel] Opening User Management window");
                var userManagementWindow = app.ServiceProvider.GetRequiredService<UserManagementWindow>();
                
                userManagementWindow.Owner = Application.Current.MainWindow;
                userManagementWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                userManagementWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                _loggingService.Error($"[MainWindowViewModel] Error opening User Management window: {ex.Message}");
                MessageBox.Show($"Error opening User Management window: {ex.Message}", 
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles user change events by refreshing access rights
        /// </summary>
        private void CurrentUserService_UserChanged(object? sender, User? e)
        {
            _loggingService.Info($"[MainWindowViewModel] User changed to: {e?.Username ?? "<null>"}");
            RefreshAccess();
        }

        /// <summary>
        /// Updates the administrator access status based on current user's rights
        /// </summary>
        public void RefreshAccess()
        {
            var hasAccess = _currentUserService.HasRequiredAccess(AccessLevel.Administrator);
            _loggingService.Info($"[MainWindowViewModel] Access refreshed - IsAdmin: {hasAccess}");
            IsAdministrator = hasAccess;
        }

        /// <summary>
        /// Notifies UI of property changes
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Cleanup: unsubscribe from events to prevent memory leaks
        /// </summary>
        public void Dispose()
        {
            _currentUserService.UserChanged -= CurrentUserService_UserChanged;
            _loggingService.Info("[MainWindowViewModel] Disposed, events cleaned up");
        }
    }
}
