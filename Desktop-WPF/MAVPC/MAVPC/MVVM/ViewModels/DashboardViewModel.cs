using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAVPC.Models;
using MAVPC.MVVM.Views;
using MAVPC.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel; // [NUEVO] Necesario para ICollectionView
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data; // [NUEVO] Necesario para CollectionViewSource
using System.Windows.Media;

namespace MAVPC.MVVM.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ITrafficService _trafficService;

        // --- COLECCIONES ---
        [ObservableProperty] private ObservableCollection<Camara> _cameras;
        [ObservableProperty] private ObservableCollection<Incidencia> _incidencias;

        // [NUEVO] VISTA FILTRABLE PARA EL XAML
        public ICollectionView CamerasView { get; private set; }

        // [NUEVO] TEXTO DE BÚSQUEDA
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    // Cada vez que escribes, refrescamos el filtro
                    CamerasView?.Refresh();
                }
            }
        }

        // --- KPIs Y ESTADO ---
        [ObservableProperty] private int _totalCameras;
        [ObservableProperty] private int _activeIncidents;
        [ObservableProperty] private string _systemStatus = "CONECTADO";
        [ObservableProperty] private bool _isLoading;

        public DashboardViewModel(ITrafficService trafficService)
        {
            _trafficService = trafficService;
            Cameras = new ObservableCollection<Camara>();
            Incidencias = new ObservableCollection<Incidencia>();

            // Carga inicial
            LoadDataCommand.Execute(null);
        }

        [RelayCommand]
        private async Task LoadData()
        {
            if (IsLoading) return;
            IsLoading = true;
            SystemStatus = "SINCRONIZANDO...";

            try
            {
                // 1. Cargar Cámaras
                var dataCam = await _trafficService.GetCamarasAsync();
                Cameras.Clear();
                if (dataCam != null) foreach (var item in dataCam) Cameras.Add(item);

                // [NUEVO] Inicializar la Vista Filtrable sobre la lista de Cámaras
                CamerasView = CollectionViewSource.GetDefaultView(Cameras);
                CamerasView.Filter = FilterCameras; // Asignamos la función de filtrado
                OnPropertyChanged(nameof(CamerasView)); // Avisamos al XAML

                // 2. Cargar Incidencias
                var dataInc = await _trafficService.GetIncidenciasAsync();
                Incidencias.Clear();
                if (dataInc != null) foreach (var item in dataInc) Incidencias.Add(item);

                // 3. Actualizar KPIs
                TotalCameras = Cameras.Count;
                ActiveIncidents = Incidencias.Count;
                SystemStatus = "EN LÍNEA";
            }
            catch (Exception ex)
            {
                SystemStatus = "ERROR DE RED";
                MessageBox.Show($"Error al cargar datos: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // [NUEVO] LÓGICA DEL FILTRO
        private bool FilterCameras(object item)
        {
            if (item is Camara cam)
            {
                // Si el buscador está vacío, mostrar todo
                if (string.IsNullOrWhiteSpace(SearchText))
                    return true;

                string search = SearchText.ToLower();

                // Buscamos por Nombre, Carretera o Kilómetro
                // Usamos ?.ToString() para evitar errores si algún dato viene nulo
                return (cam.Nombre != null && cam.Nombre.ToLower().Contains(search)) ||
                       (cam.Carretera != null && cam.Carretera.ToLower().Contains(search)) ||
                       (cam.Kilometro != null && cam.Kilometro.ToString().Contains(search));
            }
            return false;
        }

        [RelayCommand]
        private async Task OpenAddForm()
        {
            var window = new Window
            {
                Title = "Añadir Nuevo Punto",
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            Action closeAction = () => window.Close();
            var addItemVm = new AddItemViewModel(_trafficService, closeAction);
            var view = new AddItemView { DataContext = addItemVm };

            window.Content = view;
            window.ShowDialog();

            await LoadData();
        }

        [RelayCommand]
        private void ExportPdf()
        {
            MessageBox.Show("Función de PDF pendiente de implementar.", "Info");
        }
    }
}