using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAVPC.Models;
using MAVPC.MVVM.Views;
using MAVPC.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MAVPC.MVVM.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ITrafficService _trafficService;
        private readonly IPdfService _pdfService;

        // --- COLECCIONES ---
        [ObservableProperty] private ObservableCollection<Camara> _cameras;
        [ObservableProperty] private ObservableCollection<Incidencia> _incidencias;

        // Vista filtrable para rendimiento en búsqueda
        public ICollectionView CamerasView { get; private set; }

        // --- BÚSQUEDA ---
        private string _searchText = string.Empty;
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

        // --- KPIs ---
        [ObservableProperty] private int _totalCameras;
        [ObservableProperty] private int _activeIncidents;
        [ObservableProperty] private string _systemStatus = "CONECTADO";
        [ObservableProperty] private bool _isLoading;

        public DashboardViewModel(ITrafficService trafficService, IPdfService pdfService)
        {
            _trafficService = trafficService;
            _pdfService = pdfService;

            Cameras = new ObservableCollection<Camara>();
            Incidencias = new ObservableCollection<Incidencia>();

            // Inicialización asíncrona segura (Fire and Forget controlado)
            _ = LoadData();
        }

        [RelayCommand]
        private async Task LoadData()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                SystemStatus = "SINCRONIZANDO...";

                // 1. Carga paralela para velocidad
                var taskCam = _trafficService.GetCamarasAsync();
                var taskInc = _trafficService.GetIncidenciasAsync();

                await Task.WhenAll(taskCam, taskInc);

                var dataCam = await taskCam;
                var dataInc = await taskInc;

                // 2. Actualizar UI en hilo principal
                UpdateCameras(dataCam);
                UpdateIncidencias(dataInc);

                // 3. KPIs
                TotalCameras = Cameras.Count;
                ActiveIncidents = Incidencias.Count;
                SystemStatus = "EN LÍNEA";
            }
            catch (Exception ex)
            {
                SystemStatus = "ERROR RED";
                MessageBox.Show($"Error de sincronización: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void UpdateCameras(System.Collections.Generic.IEnumerable<Camara>? data)
        {
            Cameras.Clear();
            if (data != null)
            {
                foreach (var item in data) Cameras.Add(item);
            }

            if (CamerasView == null)
            {
                CamerasView = CollectionViewSource.GetDefaultView(Cameras);
                CamerasView.Filter = FilterCameras;
            }
            CamerasView.Refresh();
            OnPropertyChanged(nameof(CamerasView));
        }

        private void UpdateIncidencias(System.Collections.Generic.IEnumerable<Incidencia>? data)
        {
            Incidencias.Clear();
            if (data != null)
            {
                foreach (var item in data) Incidencias.Add(item);
            }
        }

        private bool FilterCameras(object item)
        {
            if (string.IsNullOrWhiteSpace(SearchText)) return true;
            if (item is not Camara cam) return false;

            // Búsqueda optimizada (Case Insensitive sin crear strings nuevos)
            return (cam.Nombre?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                   (cam.Carretera?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true) ||
                   (cam.Kilometro?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true);
        }

        // --- COMANDOS DE ACCIÓN ---

        [RelayCommand]
        private void ViewCamera(Camara camara)
        {
            if (camara == null) return;

            // Abrimos la ventana de detalle (CameraWindow debe existir en Views)
            var ventana = new CameraWindow(camara); // Asegúrate que CameraWindow tiene este constructor
            ventana.Owner = Application.Current.MainWindow;
            ventana.ShowDialog();
        }

        [RelayCommand]
        private async Task DeleteCamera(Camara camara)
        {
            if (camara == null) return;

            var result = MessageBox.Show(
                $"¿Eliminar cámara '{camara.Nombre}'?",
                "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                bool exito = await _trafficService.DeleteCamaraAsync(camara.Id);
                if (exito)
                {
                    Cameras.Remove(camara);
                    CamerasView.Refresh();
                    TotalCameras--;
                }
                else
                {
                    MessageBox.Show("Error al eliminar en el servidor.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task OpenAddForm()
        {
            // Creamos una ventana contenedora limpia para el UserControl AddItemView
            var window = new Window
            {
                Title = "Añadir Punto",
                SizeToContent = SizeToContent.WidthAndHeight,
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Background = Brushes.Transparent,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Owner = Application.Current.MainWindow
            };

            // Inyectamos dependencias
            var vm = new AddItemViewModel(_trafficService, () => window.Close());
            var view = new AddItemView { DataContext = vm };
            window.Content = view;

            window.ShowDialog();

            // Recargamos datos al cerrar por si hubo cambios
            await LoadData();
        }

        [RelayCommand]
        private async Task ExportPdf()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "PDF Files (*.pdf)|*.pdf",
                FileName = $"Reporte_MAVPC_{DateTime.Now:yyyyMMdd_HHmm}.pdf"
            };

            if (dialog.ShowDialog() != true) return;

            try
            {
                SystemStatus = "GENERANDO PDF...";
                IsLoading = true;

                // Obtenemos historial completo para el reporte
                var historial = await _trafficService.GetAllIncidenciasAsync();

                if (historial == null || !historial.Any())
                {
                    MessageBox.Show("No hay datos históricos para generar el reporte.");
                    return;
                }

                // Generación en segundo plano (CPU bound)
                await Task.Run(() => _pdfService.GenerateFullReport(dialog.FileName, historial));

                // Abrir PDF resultante
                var p = new System.Diagnostics.Process();
                p.StartInfo = new System.Diagnostics.ProcessStartInfo(dialog.FileName) { UseShellExecute = true };
                p.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generando PDF: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
                SystemStatus = "EN LÍNEA";
            }
        }
    }
}