using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;
using System.Windows;

namespace MAVPC.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        // SOLUCIÓN ERRORES NULL: Inicializamos con string.Empty
        [ObservableProperty]
        private string _usuario = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _mensajeError = string.Empty;

        [ObservableProperty]
        private bool _isCargando;

        [RelayCommand]
        private async Task Login()
        {
            IsCargando = true;
            MensajeError = string.Empty;

            // Simulación de espera
            await Task.Delay(1000);

            // Validación simple
            if (Usuario == "admin" && Password == "1234")
            {
                MessageBox.Show("¡Login Correcto!", "Sistema MAVPC");
                // Aquí navegaremos al Dashboard más adelante
            }
            else
            {
                MensajeError = "Usuario o contraseña incorrectos";
            }

            IsCargando = false;
        }
    }
}