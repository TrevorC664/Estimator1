using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Estimator1.Core.Interfaces;
using Estimator1.Core.Models;
using Estimator1.WPF.Commands;

namespace Estimator1.WPF.ViewModels
{
    public class LoginViewModel : LoginWindowViewModel
    {
        public LoginViewModel(IAuthenticationService authService, ILoggingService loggingService, 
            ICurrentUserService currentUserService, IServiceProvider serviceProvider)
            : base(authService, serviceProvider, currentUserService, loggingService)
        {
        }
    }
}
