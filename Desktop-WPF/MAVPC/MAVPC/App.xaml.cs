using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;
// Namespaces propios
using MAVPC.MVVM.ViewModels;
using MAVPC.MVVM.Views;
using MAVPC.Services;

namespace MAVPC
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// Configura el contenedor de inyección de dependencias (DI) y el inicio.
    /// </summary>
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Configuración centralizada de servicios (IoC Container).
        /// </summary>
        private void ConfigureServices(IServiceCollection services)
        {
            // 1. CORE & HTTP
            services.AddHttpClient();

            // 2. SERVICIOS (Lógica de Negocio)
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<ITrafficService, TrafficService>();
            services.AddSingleton<IPdfService, PdfService>();

            // 3. VIEWMODELS
            // Singleton: Se mantiene vivo toda la sesión (estado global de la UI principal)
            services.AddSingleton<MainViewModel>();

            // Transient: Se crea una instancia nueva cada vez que se pide
            services.AddTransient<LoginViewModel>();
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<MapViewModel>();
            services.AddTransient<StatsViewModel>();
            services.AddTransient<UsersViewModel>();

            // 4. VISTAS (Inyección manual del DataContext)
            services.AddSingleton<MainView>(provider => new MainView
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            });
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                // Configuración de Licencia QuestPDF (Requerido para evitar marca de agua/excepciones)
                QuestPDF.Settings.License = LicenseType.Community;

                // Resolución e inicio de la ventana principal
                var mainWindow = _serviceProvider.GetRequiredService<MainView>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fatal al iniciar: {ex.Message}", "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }

        /// <summary>
        /// Captura errores no controlados en el hilo de la UI para evitar cierres abruptos sin log.
        /// </summary>
        private void App_OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // Aquí deberías conectar un Logger (Serilog/NLog)
            System.Diagnostics.Debug.WriteLine($"[CRASH] {e.Exception.Message}");

            MessageBox.Show($"Ocurrió un error inesperado:\n{e.Exception.Message}", "Error de Aplicación", MessageBoxButton.OK, MessageBoxImage.Error);

            e.Handled = true; // Evita que la app se cierre si el error no es catastrófico
        }
    }
}