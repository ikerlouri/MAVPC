using MAVPC.MVVM.ViewModels;
using MAVPC.MVVM.Views;
using MAVPC.Services;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure; // <--- IMPORTANTE
using System.Windows;

namespace MAVPC
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();

            // HTTP Client
            services.AddHttpClient();

            // Servicios
            services.AddSingleton<IAuthService, AuthService>();
            services.AddSingleton<ITrafficService, TrafficService>();

            // --- NUEVO: REGISTRAR EL SERVICIO DE PDF ---
            services.AddSingleton<IPdfService, PdfService>();
            // -------------------------------------------

            // ViewModels
            services.AddSingleton<MainViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<DashboardViewModel>(); // Se inyectará IPdfService automáticamente aquí
            services.AddTransient<MapViewModel>();
            services.AddTransient<StatsViewModel>();
            services.AddTransient<UsersViewModel>();

            // Vistas
            services.AddSingleton<MainView>(provider => new MainView
            {
                DataContext = provider.GetRequiredService<MainViewModel>()
            });

            _serviceProvider = services.BuildServiceProvider();
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            // --- NUEVO: LICENCIA QUESTPDF (Requerido) ---
            QuestPDF.Settings.License = LicenseType.Community;
            // --------------------------------------------

            var mainWindow = _serviceProvider.GetRequiredService<MainView>();
            mainWindow.Show();
        }
    }
}
============================================================
ARCHIVO: C:\Users\2dam3\Documents\Retos\MAVPC\Desktop-WPF\MAVPC\MAVPC\App.xaml.cs
============================================================
