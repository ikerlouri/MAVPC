using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace MAVPC.MVVM.ViewModels
{
    /// <summary>
    /// ViewModel principal de la aplicación.
    /// Gestiona la navegación entre vistas, el estado de autenticación y los controles de la ventana.
    /// Actúa como orquestador usando el contenedor de inyección de dependencias.
    /// </summary>
    public partial class MainViewModel : ObservableObject, IRecipient<LoginSuccessMessage>
    {
        // Contenedor de servicios para instanciar ViewModels bajo demanda (Navegación Lazy)
        private readonly IServiceProvider _provider;

        /// <summary>
        /// Vista actual mostrada en el ContentControl de la ventana principal.
        /// </summary>
        [ObservableProperty]
        private object? _currentView;

        /// <summary>
        /// Indica si el usuario ha iniciado sesión. Controla la visibilidad del menú lateral.
        /// </summary>
        [ObservableProperty]
        private bool _isLoggedIn;

        public MainViewModel(IServiceProvider provider)
        {
            _provider = provider;

            // Nos suscribimos al mensajero para escuchar cuando alguien inicie sesión correctamente
            WeakReferenceMessenger.Default.Register(this);

            // Estado inicial: Mostramos el Login y ocultamos el menú
            CurrentView = _provider.GetRequiredService<LoginViewModel>();
            IsLoggedIn = false;
        }

        /// <summary>
        /// Método invocado automáticamente cuando llega un mensaje 'LoginSuccessMessage'.
        /// </summary>
        /// <param name="message">Datos del login (usuario, token, etc.)</param>
        public void Receive(LoginSuccessMessage message)
        {
            IsLoggedIn = true;
            // Al loguearse, redirigimos automáticamente al Dashboard
            CurrentView = _provider.GetRequiredService<DashboardViewModel>();
        }

        #region Comandos de Navegación

        [RelayCommand]
        private void ShowDashboard()
        {
            CurrentView = _provider.GetRequiredService<DashboardViewModel>();
        }

        [RelayCommand]
        private void ShowMap()
        {
            CurrentView = _provider.GetRequiredService<MapViewModel>();
        }

        [RelayCommand]
        private void ShowStats()
        {
            CurrentView = _provider.GetRequiredService<StatsViewModel>();
        }

        [RelayCommand]
        private void ShowUsers()
        {
            CurrentView = _provider.GetRequiredService<UsersViewModel>();
        }

        [RelayCommand]
        private void Logout()
        {
            IsLoggedIn = false;
            // Volvemos a la pantalla de login
            CurrentView = _provider.GetRequiredService<LoginViewModel>();
        }

        #endregion

        #region Comandos de Control de Ventana

        [RelayCommand]
        private void CloseApp() => Application.Current.Shutdown(0);

        // Nota: Los comandos de Min/Max suelen requerir lógica de vista, 
        // pero aquí los lanzamos para mantener el patrón MVVM puro donde sea posible.

        [RelayCommand]
        private void MinimizeApp()
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        [RelayCommand]
        private void MaximizeRestoreApp()
        {
            var win = Application.Current.MainWindow;
            if (win.WindowState == WindowState.Maximized)
                win.WindowState = WindowState.Normal;
            else
                win.WindowState = WindowState.Maximized;
        }

        #endregion
    }
}