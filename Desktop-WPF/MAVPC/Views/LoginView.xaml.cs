using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MAVPC.ViewModels;

namespace MAVPC.Views
{
    public partial class LoginView : Window
    {
        public LoginView()
        {
            InitializeComponent();
        }

        // Permite mover la ventana arrastrando el fondo negro
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        // Cierra la aplicación
        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // MAGIA: Esto pasa la contraseña al ViewModel de forma segura y sencilla
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is LoginViewModel vm)
            {
                vm.Password = ((PasswordBox)sender).Password;
            }
        }
    }
}