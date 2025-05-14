using System;
using System.Threading.Tasks;

namespace Estimator1.Core.Interfaces
{
    public interface ILoggingService
    {
        void Debug(string message);
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Error(Exception ex, string message);
        void Fatal(string message);
        void Fatal(Exception ex, string message);
        Task LogSecurityEventAsync(int userId, string eventType, string details);
    }
}
