using System;
using System.Windows;
using Estimator1.Core.Interfaces;
using Estimator1.WPF.ViewModels;

namespace Estimator1.WPF.Views
{
    /// <summary>
    /// Main application window providing access to user management and application exit
    /// Controls access to features based on user's administrator status
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ILoggingService _loggingService;
        private readonly MainWindowViewModel _viewModel;

        /// <summary>
        /// Initializes the main window with dependency-injected services
        /// </summary>
        public MainWindow(MainWindowViewModel viewModel, ILoggingService loggingService)
        {
            InitializeComponent();  // Initialize XAML components
            _loggingService = loggingService;
            _viewModel = viewModel;

            // Bind the view model to enable XAML data binding
            DataContext = _viewModel;
            _loggingService.Info("[MainWindow] Window initialized and DataContext set");

            // Center the window on screen and set up initial event handlers
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Loaded += MainWindow_Loaded;
        }

        /// <summary>
        /// Handles window loaded event - ensures proper access rights are set
        /// and window has focus
        /// </summary>
        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            _loggingService.Info("[MainWindow] Window loaded, refreshing access");
            _viewModel.RefreshAccess();  // Update admin access status

            // Ensure window is properly focused for user interaction
            Activate();
            Focus();
        }

        /// <summary>
        /// Cleanup when window is closed - unsubscribe from events
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Loaded -= MainWindow_Loaded;  // Prevent memory leaks
            _loggingService.Info("[MainWindow] Window closed, events cleaned up");
        }
    }
}
