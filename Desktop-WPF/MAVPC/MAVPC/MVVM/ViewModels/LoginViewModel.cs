using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using MAVPC.Services;
using System;
using System.Windows.Controls;

namespace MAVPC.MVVM.ViewModels
{
    // Mensaje para notificar al MainViewModel que cambie la vista
    public class LoginSuccessMessage : ValueChangedMessage<string>
    {
        public LoginSuccessMessage(string user) : base(user) { }
    }

    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        // CORRECCIÓN: Quitamos [NotifyPropertyChangedFor]
        // [ObservableProperty] genera automáticamente "public bool HasError" 
        // y se encarga de notificar los cambios por sí solo.
        [ObservableProperty]
        private bool _hasError;

        public LoginViewModel(IAuthService authService)
        {
            _authService = authService;
        }

        [RelayCommand]
        private void Login(object parameter)
        {
            // Validación de seguridad: PasswordBox no permite binding directo seguro
            if (parameter is not PasswordBox passwordBox) return;

            try
            {
                var password = passwordBox.Password;

                // Validación básica de UI
                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
                {
                    ShowError("Introduzca usuario y contraseña.");
                    return;
                }

                // Limpiar estado previo
                HasError = false;
                ErrorMessage = string.Empty;

                // Llamada al servicio
                bool isAuthenticated = _authService.Login(Username, password);

                if (isAuthenticated)
                {
                    // ÉXITO: Disparamos el mensaje para que MainViewModel cambie la vista
                    WeakReferenceMessenger.Default.Send(new LoginSuccessMessage(Username));
                }
                else
                {
                    // FALLO: Feedback visual
                    ShowError("Credenciales incorrectas.");
                    passwordBox.Password = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error de sistema: {ex.Message}");
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage = message;
            HasError = true;
        }
    }
}