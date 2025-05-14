using System.Windows;
using System.IO;
using Estimator1.Core.Interfaces;
using Estimator1.Infrastructure.Data;
using Estimator1.Infrastructure.Services;
using Estimator1.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Estimator1.WPF
{
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }
        private IConfiguration _configuration;

        public App()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            ConfigureServices(services);
            ConfigureLogging();

            ServiceProvider = services.BuildServiceProvider();
            var serviceProvider = ServiceProvider;
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Add configuration
            services.AddSingleton<IConfiguration>(_configuration);

            // Add DbContext
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(_configuration.GetConnectionString("DefaultConnection")));

            // Register services as Singletons to maintain state
            services.AddSingleton<ILoggingService, LoggingService>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();
            services.AddSingleton<ICurrentUserService, CurrentUserService>();
            services.AddSingleton<ViewModels.MainWindowViewModel>();

            // Register views and other view models as Transient
            services.AddTransient<ViewModels.LoginWindowViewModel>();
            services.AddTransient<ViewModels.UserManagementViewModel>();
            services.AddTransient<Views.LoginWindow>();
            services.AddTransient<Views.MainWindow>();
            services.AddTransient<Views.UserManagementWindow>();
        }

        private void ConfigureLogging()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Create targets
            var logfile = new NLog.Targets.FileTarget("logfile")
            {
                FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Estimator1", "logs", "all-${shortdate}.log"),
                Layout = "${longdate}|${event-properties:item=EventId_Id:whenEmpty=0}|${uppercase:${level}}|${logger}|${message} ${exception:format=tostring}"
            };

            var errorfile = new NLog.Targets.FileTarget("errorfile")
            {
                FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Estimator1", "logs", "error-${shortdate}.log"),
                Layout = "${longdate}|${event-properties:item=EventId_Id:whenEmpty=0}|${uppercase:${level}}|${logger}|${message}${newline}${exception:format=tostring}${newline}Stack Trace:${newline}${stacktrace}"
            };

            var authfile = new NLog.Targets.FileTarget("authfile")
            {
                FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Estimator1", "logs", "auth-${shortdate}.log"),
                Layout = "${longdate}|${uppercase:${level}}|${logger}|${message}"
            };

            // Rules for mapping loggers to targets            
            config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, logfile);
            config.AddRule(NLog.LogLevel.Error, NLog.LogLevel.Fatal, errorfile);

            // Auth rule with filter
            var authRule = new NLog.Config.LoggingRule("*", NLog.LogLevel.Info, authfile);
            authRule.Filters.Add(new NLog.Filters.ConditionBasedFilter
            {
                Condition = "contains('${logger}','Auth') or contains('${message}','login')",
                Action = NLog.Filters.FilterResult.Log
            });
            config.LoggingRules.Add(authRule);

            // Apply config           
            LogManager.Configuration = config;

            // Test logging
            var loggingService = new LoggingService();
            loggingService.Info("Logging system initialized");
            loggingService.Info("Logging initialized");
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var loggingService = ServiceProvider.GetRequiredService<ILoggingService>();
            loggingService.Info("Application starting up...");

            // Skip database initialization when using mock authentication
            var authService = ServiceProvider.GetService<IAuthenticationService>();
            if (!(authService is MockAuthenticationService))
            {
                try
                {
                    // Initialize database
                    using (var scope = ServiceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var logger = scope.ServiceProvider.GetRequiredService<ILoggingService>();

                        logger.Info("Checking database connection...");
                        if (!context.Database.CanConnect())
                        {
                            logger.Error("Cannot connect to database. Please check your connection string and ensure PostgreSQL is running.");
                            MessageBox.Show("Cannot connect to database. Please check if PostgreSQL is running.", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            Current.Shutdown(1);
                            return;
                        }

                        logger.Info("Database connection successful. Applying migrations...");
                        context.Database.Migrate();
                        logger.Info("Database migrations applied successfully.");

                        // Seed the database
                        var seeder = new DatabaseSeeder(context, logger);
                        seeder.SeedData();
                    }
                }
                catch (Exception ex)
                {
                    var logger = ServiceProvider.GetService<ILoggingService>();
                    logger?.Error($"Database initialization error: {ex.Message}");
                    logger?.Error($"Stack trace: {ex.StackTrace}");
                    MessageBox.Show($"Database initialization error: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Current.Shutdown(1);
                    return;
                }
            }

            var loginWindow = ServiceProvider.GetRequiredService<Views.LoginWindow>();
            loginWindow.Show();
            MainWindow = loginWindow; // Set as MainWindow
            loginWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            var loggingService = ServiceProvider?.GetService<ILoggingService>();
            loggingService?.Info("Application shutting down...");

            LogManager.Shutdown();
        }
    }
}
