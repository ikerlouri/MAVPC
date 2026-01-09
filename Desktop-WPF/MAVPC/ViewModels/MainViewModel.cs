using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace MAVPC.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        // Esta propiedad cambiará lo que se ve en el centro de la pantalla
        [ObservableProperty]
        private object? _currentView;

        [ObservableProperty]
        private string _windowTitle = "MAVPC Traffic Control";

        public MainViewModel()
        {
            // AL ARRANCAR: Cargamos el Dashboard automáticamente
            CurrentView = new DashboardViewModel();
        }

        [RelayCommand]
        private void NavigateToDashboard()
        {
            CurrentView = new DashboardViewModel();
        }

        [RelayCommand]
        private void NavigateToCameras()
        {
            // Aquí pondremos la vista de cámaras más adelante
            // CurrentView = new CamerasViewModel();
            MessageBox.Show("Módulo de Cámaras en construcción");
        }

        [RelayCommand]
        private void CerrarSesion()
        {
            // Aquí podrías volver al LoginView si quisieras, de momento cerramos
            var currentWindow = Application.Current.MainWindow;
            currentWindow.Close();
        }

        // --- COMANDOS DE VENTANA ---
        [RelayCommand]
        private void MinimizeWindow() => Application.Current.MainWindow.WindowState = WindowState.Minimized;

        [RelayCommand]
        private void MaximizeWindow()
        {
            var win = Application.Current.MainWindow;
            win.WindowState = (win.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        [RelayCommand]
        private void CloseWindow() => Application.Current.Shutdown();
    }
}