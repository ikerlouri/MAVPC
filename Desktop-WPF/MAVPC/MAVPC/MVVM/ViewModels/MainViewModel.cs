using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace MAVPC.MVVM.ViewModels
{
    public partial class MainViewModel : ObservableObject, IRecipient<LoginSuccessMessage>
    {
        private readonly IServiceProvider _provider;

        [ObservableProperty] private object? _currentView;
        [ObservableProperty] private bool _isLoggedIn;

        public MainViewModel(IServiceProvider provider)
        {
            _provider = provider;
            WeakReferenceMessenger.Default.Register(this);
            // Iniciamos en Login
            CurrentView = _provider.GetRequiredService<LoginViewModel>();
            IsLoggedIn = false;
        }

        public void Receive(LoginSuccessMessage message)
        {
            IsLoggedIn = true;
            // Al hacer login, vamos al Dashboard
            CurrentView = _provider.GetRequiredService<DashboardViewModel>();
        }

        // --- COMANDOS DE NAVEGACIÓN ---

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

        // NUEVO: Comando para mostrar Usuarios
        [RelayCommand]
        private void ShowUsers()
        {
            CurrentView = _provider.GetRequiredService<UsersViewModel>();
        }

        // ------------------------------

        [RelayCommand]
        private void Logout()
        {
            IsLoggedIn = false;
            CurrentView = _provider.GetRequiredService<LoginViewModel>();
        }

        [RelayCommand] private void CloseApp() => Application.Current.Shutdown(0);
    }
}