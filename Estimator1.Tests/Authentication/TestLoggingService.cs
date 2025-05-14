using System;
using System.Threading.Tasks;
using Estimator1.Core.Interfaces;

namespace Estimator1.Tests.Authentication
{
    public class TestLoggingService : ILoggingService
    {
        public void Debug(string message)
        {
            Console.WriteLine($"[DEBUG] {message}");
        }

        public void Info(string message)
        {
            Console.WriteLine($"[INFO] {message}");
        }

        public void Warn(string message)
        {
            Console.WriteLine($"[WARN] {message}");
        }

        public void Error(string message)
        {
            Console.WriteLine($"[ERROR] {message}");
        }

        public void Error(Exception ex, string message)
        {
            Console.WriteLine($"[ERROR] {message} - Exception: {ex.Message}");
        }

        public void Fatal(string message)
        {
            Console.WriteLine($"[FATAL] {message}");
        }

        public void Fatal(Exception ex, string message)
        {
            Console.WriteLine($"[FATAL] {message} - Exception: {ex.Message}");
        }

        public Task LogSecurityEventAsync(int userId, string eventType, string details)
        {
            Console.WriteLine($"[SECURITY] User: {userId}, Type: {eventType}, Details: {details}");
            return Task.CompletedTask;
        }
    }
}
