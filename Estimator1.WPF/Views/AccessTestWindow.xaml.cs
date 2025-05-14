using System.Windows;
using Estimator1.Core.Interfaces;
using Estimator1.WPF.ViewModels;

namespace Estimator1.WPF.Views
{
    public partial class AccessTestWindow : Window
    {
        private readonly AccessTestViewModel _viewModel;
        private readonly ILoggingService _loggingService;

        public AccessTestWindow(AccessTestViewModel viewModel, ILoggingService loggingService)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _loggingService = loggingService;
            DataContext = _viewModel;
            
            _loggingService.Info($"[Auth] AccessTestWindow initialized");
            this.Loaded += (s, e) => _loggingService.Info($"[Auth] AccessTestWindow loaded and visible");
        }
    }
}
