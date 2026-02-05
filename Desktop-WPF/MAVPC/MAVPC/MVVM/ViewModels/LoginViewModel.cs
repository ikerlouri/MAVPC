using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using MAVPC.Services;
using System.Windows.Controls;

namespace MAVPC.MVVM.ViewModels
{
    public class LoginSuccessMessage : ValueChangedMessage<string>
    {
        public LoginSuccessMessage(string user) : base(user) { }
    }

    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty] private string _username = string.Empty;
        [ObservableProperty] private string _errorMessage = string.Empty;
        [ObservableProperty] private bool _hasError;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        // CAMBIO: Quitamos 'async Task' y lo dejamos en 'void'
        // porque el login local es instantáneo.
        [RelayCommand]
        private void Login(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            var password = passwordBox?.Password;

            // Limpiar errores previos
            HasError = false;
            ErrorMessage = string.Empty;

            // Llamada síncrona directa (sin await)
            if (_authService.Login(Username, password))
            {
                WeakReferenceMessenger.Default.Send(new LoginSuccessMessage(Username));
            }
            else
            {
                ErrorMessage = "Credenciales incorrectas (Prueba admin/admin)";
                HasError = true;
            }
        }
    }
}

