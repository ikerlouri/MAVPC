using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace MAVPC.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _username = string.Empty;

        // --- ¡AQUÍ ESTABA EL ERROR! Faltaba esta propiedad ---
        [ObservableProperty]
        private string _password = string.Empty;
        // -----------------------------------------------------

        [ObservableProperty]
        private bool _hasError;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [RelayCommand]
        private void Login()
        {
            // Ahora validamos también la contraseña
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Usuario y contraseña requeridos.";
                HasError = true;
            }
            else
            {
                HasError = false;
                MessageBox.Show($"Login Correcto.\nUsuario: {Username}\nPass: {Password}");
                // Aquí iría la navegación al MainView
            }
        }

        [RelayCommand]
        private void Exit()
        {
            Application.Current.Shutdown();
        }
    }
}