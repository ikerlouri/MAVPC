using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAVPC.Models;
using MAVPC.Services;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data; // Necesario para el convertidor
using System.Windows.Media;

namespace MAVPC.MVVM.ViewModels
{
    public partial class AddItemViewModel : ObservableObject
    {
        public AddItemViewModel()
        {
            // Este constructor evita el crash de TargetInvocationException
            // cuando el XAML intenta previsualizar o cargar sin dependencias.
        }
        private readonly ITrafficService _trafficService;
        private readonly Action _closeWindow;

        // Estado de carga
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private bool _isBusy;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentColor))]
        [NotifyPropertyChangedFor(nameof(IsCameraMode))]
        private bool _isCameraSelected = true;

        public bool IsCameraMode => IsCameraSelected;

        public Brush CurrentColor => IsCameraSelected
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FFFF")) // Cyan
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF007F")); // Pink

        // Listas de autocompletado
        public ObservableCollection<string> SugerenciasCiudades { get; } = new()
        {
            "Irun", "Donostia", "Bilbao", "Vitoria-Gasteiz", "Errenteria",
            "Eibar", "Zarautz", "Hernani", "Tolosa", "Lasarte-Oria", "Pasaia"
        };

        public ObservableCollection<string> SugerenciasProvincias { get; } = new()
        {
            "Gipuzkoa", "Bizkaia", "Araba", "Navarra", "Lapurdi"
        };

        // Campos Cámara
        [ObservableProperty] private string _nombreCamara = string.Empty;
        [ObservableProperty] private string _urlCamara = string.Empty;
        [ObservableProperty] private string _carreteraCamara = string.Empty;
        [ObservableProperty] private string _kmCamara = string.Empty;
        [ObservableProperty] private string _direccionCamara = "Ubicación Sistema";
        [ObservableProperty] private string _latitudCamara = "43.3400";
        [ObservableProperty] private string _longitudCamara = "-1.7900";

        // Campos Incidencia
        [ObservableProperty] private string _typeIncidence = string.Empty;
        [ObservableProperty] private string _incidenceLevel = "Verde";
        [ObservableProperty] private string _roadIncidence = string.Empty;
        [ObservableProperty] private string _directionIncidence = "Ambos";
        [ObservableProperty] private string _descriptionIncidence = string.Empty;
        [ObservableProperty] private string _cityIncidence = "Irun";
        [ObservableProperty] private string _provinceIncidence = "Gipuzkoa";
        [ObservableProperty] private string _latitudInc = "43.3400";
        [ObservableProperty] private string _longitudInc = "-1.7900";

        public AddItemViewModel(ITrafficService trafficService, Action closeWindow)
        {
            _trafficService = trafficService;
            _closeWindow = closeWindow;
        }

        [RelayCommand] private void Close() => _closeWindow?.Invoke();
        [RelayCommand] private void SwitchMode(string mode) => IsCameraSelected = (mode == "Camera");

        private double ParseCoordinate(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return 0.0;
            string normalized = input.Replace(',', '.');
            if (double.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                return result;
            return 0.0;
        }

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task Submit()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                bool success = false;

                if (IsCameraSelected)
                {
                    if (string.IsNullOrWhiteSpace(NombreCamara)) { MessageBox.Show("Nombre obligatorio"); return; }

                    var newCam = new Camara
                    {
                        Id = "CAM-" + Guid.NewGuid().ToString().Substring(0, 5).ToUpper(),
                        Nombre = NombreCamara,
                        UrlImagen = string.IsNullOrWhiteSpace(UrlCamara) ? "http://10.10.16.93:8080/images/default.jpg" : UrlCamara,
                        Carretera = CarreteraCamara,
                        Kilometro = KmCamara,
                        Direccion = DireccionCamara,
                        Latitud = LatitudCamara,
                        Longitud = LongitudCamara
                    };
                    success = await _trafficService.AddCamaraAsync(newCam);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(TypeIncidence)) { MessageBox.Show("Tipo obligatorio"); return; }

                    var newInc = new Incidencia
                    {
                        IncidenceId = "0",
                        IncidenceType = TypeIncidence,
                        IncidenceLevel = IncidenceLevel,
                        Road = RoadIncidence,
                        CityTown = CityIncidence,
                        Province = ProvinceIncidence,
                        Cause = DescriptionIncidence,
                        Direction = DirectionIncidence,
                        Latitude = ParseCoordinate(LatitudInc),
                        Longitude = ParseCoordinate(LongitudInc),
                        StartDate = DateTime.Now
                    };
                    success = await _trafficService.AddIncidenciaAsync(newInc);
                }

                if (success) { await Task.Delay(500); Close(); }
                else { MessageBox.Show("Error al guardar en el servidor."); }
            }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); }
            finally { IsBusy = false; }
        }

        private bool CanSubmit() => !IsBusy;
    }

    // --- CONVERTIDOR AÑADIDO AQUÍ PARA EVITAR ERRORES ---
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue) return !booleanValue;
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue) return !booleanValue;
            return value;
        }
    }
}

