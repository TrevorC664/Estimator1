using System.Windows;
using Estimator1.Core.Interfaces;
using Estimator1.WPF.ViewModels;

namespace Estimator1.WPF.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginWindowViewModel _viewModel;

        public LoginWindow(LoginWindowViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }
    }
}
