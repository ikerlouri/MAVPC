using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAVPC.Models;
using MAVPC.MVVM.Views;
using MAVPC.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq; // Necesario para GroupBy en el PDF
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MAVPC.MVVM.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ITrafficService _trafficService;
        private readonly IPdfService _pdfService; // <--- NUEVO SERVICIO

        // --- COLECCIONES ---
        [ObservableProperty] private ObservableCollection<Camara> _cameras;
        [ObservableProperty] private ObservableCollection<Incidencia> _incidencias;

        // VISTA FILTRABLE PARA EL XAML
        public ICollectionView CamerasView { get; private set; }

        // TEXTO DE BÚSQUEDA
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    CamerasView?.Refresh();
                }
            }
        }

        // --- KPIs Y ESTADO ---
        [ObservableProperty] private int _totalCameras;
        [ObservableProperty] private int _activeIncidents;
        [ObservableProperty] private string _systemStatus = "CONECTADO";
        [ObservableProperty] private bool _isLoading;

        // CONSTRUCTOR ACTUALIZADO: Recibe ahora AMBOS servicios
        public DashboardViewModel(ITrafficService trafficService, IPdfService pdfService)
        {
            _trafficService = trafficService;
            _pdfService = pdfService; // <--- ASIGNACIÓN

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

                // Inicializar la Vista Filtrable
                if (CamerasView == null)
                {
                    CamerasView = CollectionViewSource.GetDefaultView(Cameras);
                    CamerasView.Filter = FilterCameras;
                }
                else
                {
                    CamerasView.Refresh();
                }
                OnPropertyChanged(nameof(CamerasView));

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

        // LÓGICA DEL FILTRO
        private bool FilterCameras(object item)
        {
            if (item is Camara cam)
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                    return true;

                string search = SearchText.ToLower();

                return (cam.Nombre != null && cam.Nombre.ToLower().Contains(search)) ||
                       (cam.Carretera != null && cam.Carretera.ToLower().Contains(search)) ||
                       (cam.Kilometro != null && cam.Kilometro.ToString().Contains(search));
            }
            return false;
        }

        [RelayCommand]
        private async Task DeleteCamera(object parameter)
        {
            if (parameter is not Camara camara) return;

            var result = MessageBox.Show(
                $"¿Estás seguro de que deseas eliminar la cámara '{camara.Nombre}'?\nEsta acción es irreversible.",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                bool exito = await _trafficService.DeleteCamaraAsync(camara.Id);

                if (exito)
                {
                    Cameras.Remove(camara);
                    TotalCameras = Cameras.Count;
                    CamerasView?.Refresh();
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar la cámara. Verifica la API.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error crítico al eliminar: {ex.Message}", "Excepción", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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

            // Pasamos el trafficService
            var addItemVm = new AddItemViewModel(_trafficService, closeAction);
            var view = new AddItemView { DataContext = addItemVm };

            window.Content = view;
            window.ShowDialog();

            await LoadData();
        }

        // --- EXPORTAR PDF IMPLEMENTADO ---
        [RelayCommand]
        private async Task ExportPdf()
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf",
                    FileName = $"Informe_Ejecutivo_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
                };

                if (dialog.ShowDialog() == true)
                {
                    // 1. AVISAR AL USUARIO (Porque descargar el histórico puede tardar un segundo)
                    SystemStatus = "GENERANDO PDF...";

                    // 2. DESCARGAR DATOS HISTÓRICOS (Aquí llamamos al NUEVO endpoint)
                    var historialCompleto = await _trafficService.GetAllIncidenciasAsync();

                    if (historialCompleto == null || historialCompleto.Count == 0)
                    {
                        MessageBox.Show("No se pudo descargar el historial para el reporte.", "Error");
                        SystemStatus = "EN LÍNEA";
                        return;
                    }

                    // 3. GENERAR EL REPORTE CON EL HISTORIAL
                    // (El PdfService "Pro" que hicimos antes procesará todos estos datos)
                    await Task.Run(() => _pdfService.GenerateFullReport(dialog.FileName, historialCompleto));

                    SystemStatus = "EN LÍNEA";

                    // 4. ABRIR
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = dialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                SystemStatus = "ERROR";
                MessageBox.Show($"Error generando PDF: {ex.Message}");
            }
        }
    }
}
============================================================
ARCHIVO: C:\Users\2dam3\Documents\Retos\MAVPC\Desktop-WPF\MAVPC\MAVPC\MVVM\ViewModels\DashboardViewModel.cs
============================================================
