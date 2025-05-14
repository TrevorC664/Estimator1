using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Estimator1.Core.Interfaces;
using Estimator1.Core.Models;
using Estimator1.WPF.Commands;
using Estimator1.WPF.Views;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Estimator1.WPF.ViewModels
{
    public class LoginWindowViewModel : INotifyPropertyChanged
    {
        private readonly IAuthenticationService _authService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILoggingService _loggingService;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isAccountLocked;
        private string _lockoutMessage = string.Empty;
        private int _remainingAttempts = 5;
        private bool _showRemainingAttempts;
        private int _passwordStrength;
        private string _passwordStrengthText = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public LoginWindowViewModel(IAuthenticationService authService, IServiceProvider serviceProvider, ICurrentUserService currentUserService, ILoggingService loggingService)
        {
            _authService = authService;
            _serviceProvider = serviceProvider;
            _currentUserService = currentUserService;
            _loggingService = loggingService;
            LoginCommand = new LoginCommand(this, _authService, _loggingService, _currentUserService, _serviceProvider);
        }

        public ICommand LoginCommand { get; }

        public string Username
        {
            get => _username;
            set
            {
                if (_username != value)
                {
                    _username = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                    UpdatePasswordStrength();
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsAccountLocked
        {
            get => _isAccountLocked;
            set
            {
                if (_isAccountLocked != value)
                {
                    _isAccountLocked = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LockoutMessage
        {
            get => _lockoutMessage;
            set
            {
                if (_lockoutMessage != value)
                {
                    _lockoutMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ShowRemainingAttempts
        {
            get => _showRemainingAttempts;
            set
            {
                if (_showRemainingAttempts != value)
                {
                    _showRemainingAttempts = value;
                    OnPropertyChanged();
                }
            }
        }

        public int RemainingAttempts
        {
            get => _remainingAttempts;
            set
            {
                if (_remainingAttempts != value)
                {
                    _remainingAttempts = value;
                    OnPropertyChanged();
                }
            }
        }

        public int PasswordStrength
        {
            get => _passwordStrength;
            internal set
            {
                if (_passwordStrength != value)
                {
                    _passwordStrength = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PasswordStrengthText
        {
            get => _passwordStrengthText;
            internal set
            {
                if (_passwordStrengthText != value)
                {
                    _passwordStrengthText = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Username) && 
                   !string.IsNullOrWhiteSpace(Password) && 
                   !IsAccountLocked;
        }

        private async Task LoginAsync()
        {
            try
            {
                ErrorMessage = string.Empty;
                var result = await _authService.LoginAsync(Username, Password);

                if (!result.success)
                {
                    ErrorMessage = result.message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"An error occurred during login: {ex.Message}";
                _loggingService.Error($"Login error: {ex.Message}");
            }
        }

        private void UpdatePasswordStrength()
        {
            if (string.IsNullOrEmpty(Password))
            {
                PasswordStrength = 0;
                PasswordStrengthText = string.Empty;
                return;
            }

            // Calculate password strength
            int strength = 0;
            
            if (Password.Length >= 8) strength += 25;
            if (System.Text.RegularExpressions.Regex.IsMatch(Password, @"[A-Z]")) strength += 25;
            if (System.Text.RegularExpressions.Regex.IsMatch(Password, @"[a-z]")) strength += 25;
            if (System.Text.RegularExpressions.Regex.IsMatch(Password, @"[0-9]")) strength += 25;

            PasswordStrength = strength;
            PasswordStrengthText = strength switch
            {
                0 => "Enter password",
                25 => "Weak",
                50 => "Fair",
                75 => "Good",
                100 => "Strong",
                _ => string.Empty
            };
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
