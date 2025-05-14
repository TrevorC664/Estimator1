using System;
using System.Windows;
using System.Windows.Controls;
using Estimator1.WPF.Behaviors;
using Estimator1.WPF.ViewModels;

namespace Estimator1.WPF.Views
{
    /// <summary>
    /// Interaction logic for UserManagementWindow.xaml
    /// </summary>
    public partial class UserManagementWindow : Window
    {
        private readonly UserManagementViewModel _viewModel;

        public UserManagementWindow(UserManagementViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;

            // Set up password box behavior
            Loaded += UserManagementWindow_Loaded;
        }

        private void UserManagementWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (PasswordBox != null)
            {
                PasswordBoxBehavior.Attach(PasswordBox);
                PasswordBox.PasswordChanged += PasswordBox_PasswordChanged;
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                _viewModel.Password = passwordBox.Password;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            
            if (PasswordBox != null)
            {
                PasswordBoxBehavior.Detach(PasswordBox);
                PasswordBox.PasswordChanged -= PasswordBox_PasswordChanged;
            }

            Loaded -= UserManagementWindow_Loaded;
        }
    }
}
