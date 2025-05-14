using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Windows;
using Estimator1.Core.Models;
using Estimator1.Core.Interfaces;
using Estimator1.Core.Enums;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Estimator1.Infrastructure.Data;
using Estimator1.WPF.Commands;

namespace Estimator1.WPF.ViewModels
{
    public class UserManagementViewModel : ViewModelBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILoggingService _loggingService;
        private readonly ICurrentUserService _currentUserService;
        private ObservableCollection<User> _users = new();
        private User? _selectedUser;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private AccessLevel _selectedAccessLevel = AccessLevel.Basic;

        public ObservableCollection<User> Users
        {
            get => _users;
            private set => SetField(ref _users, value);
        }

        public User? SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (SetField(ref _selectedUser, value))
                {
                    // Update form fields when selection changes
                    Username = value?.Username ?? string.Empty;
                    SelectedAccessLevel = value?.AccessLevel ?? AccessLevel.Basic;
                    // Don't set password - require new password for updates
                    UpdateCommands();
                }
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                if (SetField(ref _username, value))
                {
                    UpdateCommands();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (SetField(ref _password, value))
                {
                    UpdateCommands();
                }
            }
        }

        public AccessLevel SelectedAccessLevel
        {
            get => _selectedAccessLevel;
            set => SetField(ref _selectedAccessLevel, value);
        }

        public Array AccessLevels => Enum.GetValues(typeof(AccessLevel));

        public ICommand AddUserCommand { get; }
        public ICommand UpdateUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ClearFormCommand { get; }

        public UserManagementViewModel(
            ApplicationDbContext dbContext,
            ILoggingService loggingService,
            ICurrentUserService currentUserService)
        {
            _dbContext = dbContext;
            _loggingService = loggingService;
            _currentUserService = currentUserService;

            // Initialize commands
            AddUserCommand = new AsyncRelayCommand(AddUserAsync, CanAddUser);
            UpdateUserCommand = new AsyncRelayCommand(UpdateUserAsync, CanUpdateUser);
            DeleteUserCommand = new AsyncRelayCommand(DeleteUserAsync, CanDeleteUser);
            ClearFormCommand = new AsyncRelayCommand(ClearForm);

            _ = InitializeAsync();  // Fire and forget, but capture errors
        }

        private async Task InitializeAsync()
        {
            try
            {
                await LoadUsersAsync();
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error loading users: {ex.Message}");
                await Task.Run(() => MessageBox.Show($"Error loading users: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }

        private async Task LoadUsersAsync()
        {
            var users = await _dbContext.Users.OrderBy(u => u.Username).ToListAsync();
            Users = new ObservableCollection<User>(users);
            _loggingService.Info($"Loaded {users.Count} users");
        }

        private async Task AddUserAsync()
        {
            try
            {
                if (await _dbContext.Users.AnyAsync(u => u.Username == Username))
                {
                    MessageBox.Show("Username already exists", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var now = DateTime.UtcNow;
                var newUser = new User
                {
                    Username = Username,
                    Email = $"{Username}@estimator.local", // Default email
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password),
                    AccessLevel = SelectedAccessLevel,
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsActive = true
                };

                _dbContext.Users.Add(newUser);
                await _dbContext.SaveChangesAsync();
                _loggingService.Info($"Added new user: {Username}");

                await LoadUsersAsync();
                await ClearForm();
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error adding user: {ex.Message}");
                MessageBox.Show($"Error adding user: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UpdateUserAsync()
        {
            try
            {
                if (SelectedUser == null) return;

                // Check if username is taken by another user
                var existingUser = await _dbContext.Users
                    .FirstOrDefaultAsync(u => u.Username == Username && u.Id != SelectedUser.Id);
                if (existingUser != null)
                {
                    MessageBox.Show("Username already exists", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                SelectedUser.Username = Username;
                SelectedUser.Email = $"{Username}@estimator.local"; // Update email to match username
                SelectedUser.AccessLevel = SelectedAccessLevel;
                SelectedUser.UpdatedAt = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(Password))
                {
                    SelectedUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password);
                }

                await _dbContext.SaveChangesAsync();
                _loggingService.Info($"Updated user: {Username}");

                await LoadUsersAsync();
                await ClearForm();
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error updating user: {ex.Message}");
                MessageBox.Show($"Error updating user: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task DeleteUserAsync()
        {
            try
            {
                if (SelectedUser == null) return;

                // Prevent deleting the last administrator
                if (SelectedUser.AccessLevel == AccessLevel.Administrator)
                {
                    var adminCount = await _dbContext.Users
                        .CountAsync(u => u.AccessLevel == AccessLevel.Administrator);
                    if (adminCount <= 1)
                    {
                        MessageBox.Show("Cannot delete the last administrator", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // Prevent deleting yourself
                if (SelectedUser.Username == _currentUserService.CurrentUser?.Username)
                {
                    MessageBox.Show("Cannot delete your own account", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to delete user '{SelectedUser.Username}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _dbContext.Users.Remove(SelectedUser);
                    await _dbContext.SaveChangesAsync();
                    _loggingService.Info($"Deleted user: {SelectedUser.Username}");

                    await LoadUsersAsync();
                    await ClearForm();
                }
            }
            catch (Exception ex)
            {
                _loggingService.Error($"Error deleting user: {ex.Message}");
                MessageBox.Show($"Error deleting user: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Task ClearForm()
        {
            SelectedUser = null;
            Username = string.Empty;
            Password = string.Empty;
            SelectedAccessLevel = AccessLevel.Basic;
            return Task.CompletedTask;
        }

        private bool CanAddUser()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password);
        }

        private bool CanUpdateUser()
        {
            return SelectedUser != null &&
                   !string.IsNullOrWhiteSpace(Username);
        }

        private bool CanDeleteUser()
        {
            return SelectedUser != null;
        }

        private void UpdateCommands()
        {
            (AddUserCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (UpdateUserCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (DeleteUserCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
