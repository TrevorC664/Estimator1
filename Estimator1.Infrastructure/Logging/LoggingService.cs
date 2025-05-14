using Estimator1.Core.Interfaces;
using NLog;

using System.Threading.Tasks;

namespace Estimator1.Infrastructure.Logging
{
    public class LoggingService : ILoggingService
    {
        private readonly ILogger _logger;

        public LoggingService()
        {
            _logger = LogManager.GetCurrentClassLogger();
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void Warn(string message)
        {
            _logger.Warn(message);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }

        public void Error(Exception ex, string message)
        {
            _logger.Error(ex, message);
        }

        public void Fatal(string message)
        {
            _logger.Fatal(message);
        }

        public void Fatal(Exception ex, string message)
        {
            _logger.Fatal(ex, message);
        }

        public Task LogSecurityEventAsync(int userId, string eventType, string details)
        {
            var logEntry = $"Security Event - User:{userId} - {eventType} - {details}";
            _logger.Info(logEntry);
            return Task.CompletedTask;
        }
    }
}
