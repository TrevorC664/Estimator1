using System;
using System.Globalization;
using System.Windows.Data;
using Estimator1.Core.Interfaces;

namespace Estimator1.WPF.Converters
{
    public class BoolToStringConverter : IValueConverter
    {
        private readonly ILoggingService _loggingService;

        public BoolToStringConverter(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                _loggingService.Info($"[BoolToStringConverter] Converting bool value: {boolValue}");
                return boolValue.ToString();
            }
            _loggingService.Warn($"[BoolToStringConverter] Received non-bool value: {value?.GetType().Name ?? "null"}");
            return "false";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
