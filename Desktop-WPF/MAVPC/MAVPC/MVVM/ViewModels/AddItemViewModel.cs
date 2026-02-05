using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAVPC.Models;
using MAVPC.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MAVPC.MVVM.ViewModels
{
    public partial class AddItemViewModel : ObservableObject
    {
        private readonly ITrafficService _trafficService;
        private readonly Action _closeWindow;

        // --- CONSTANTES ---
        private const string DEFAULT_CAM_URL = "http://10.10.16.93:8080/images/default.jpg";
        private static readonly Color ColorCamera = (Color)ColorConverter.ConvertFromString("#00FFFF"); // Cyan
        private static readonly Color ColorIncidence = (Color)ColorConverter.ConvertFromString("#FF007F"); // Pink

        // --- PROVINCIAS Y MUNICIPIOS (Diccionario para búsqueda rápida) ---
        private static readonly Dictionary<string, string[]> MapaMunicipios = new()
        {
            { "Bizkaia", new[] { "bilbao", "barakaldo", "getxo", "portugalete", "santurtzi", "basauri", "leioa", "galdakao", "durango", "sestao", "erandio", "amorebieta", "gernika", "bermeo", "mungia", "sopela", "arrigorriaga", "trapagaran", "etxebarri", "abanto", "ondarroa", "ortuella", "balmaseda", "muskiz", "lekeitio", "berriz", "güeñes", "derio", "gorliz", "orozko", "zalla", "elorrio", "lemoa", "plentzia" } },
            { "Gipuzkoa", new[] { "donostia", "san sebastián", "irun", "errenteria", "eibar", "zarautz", "arrasate", "mondragón", "hernani", "tolosa", "lasarte", "pasaia", "bergara", "azpeitia", "beasain", "andoain", "oñati", "zumarraga", "hondarribia", "elgoibar", "oiartzun", "azkoitia", "lazkao", "mutriku", "urniet", "deba", "zumaia", "lezo", "villabona", "astigarraga", "aretxabaleta", "oria", "getaria", "orio" } },
            { "Araba", new[] { "vitoria", "gasteiz", "laudio", "llodio", "amurrio", "salvatierra", "agurain", "oion", "oyón", "iruña de oca", "ayala", "aiara", "zuia", "legutio", "aramaio", "laguardia", "labastida" } },
            { "Navarra", new[] { "pamplona", "iruña", "tudela", "barañáin", "burlada", "egüés", "estella", "lizarra", "tafalla", "alsasua", "altsasu", "baztan", "corella", "noáin", "cintruénigo", "bera", "lesaka", "leitza", "etxarri" } },
            { "Lapurdi", new[] { "hendaya", "hendaia", "bayonne", "baiona", "biarritz" } }
        };

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private bool _isBusy;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentColor))]
        [NotifyPropertyChangedFor(nameof(IsCameraMode))]
        private bool _isCameraSelected = true;

        public bool IsCameraMode => IsCameraSelected;

        // Usamos pinceles congelados si es posible, o creados al vuelo pero eficientes
        public Brush CurrentColor => IsCameraSelected
            ? new SolidColorBrush(ColorCamera)
            : new SolidColorBrush(ColorIncidence);

        // --- FECHA Y HORA ---
        [ObservableProperty] private DateTime _selectedDate = DateTime.Now;
        [ObservableProperty] private DateTime _selectedTime = DateTime.Now;

        // --- LISTAS AUTOCOMPLETADO ---
        public ObservableCollection<string> SugerenciasCiudades { get; } = new()
        {
            "Bilbao", "Donostia/San Sebastián", "Vitoria-Gasteiz", "Pamplona/Iruña",
            "Irun", "Errenteria", "Eibar", "Zarautz", "Hernani", "Tolosa",
            "Barakaldo", "Getxo", "Portugalete", "Santurtzi", "Basauri",
            "Laudio/Llodio", "Amurrio", "Agurain/Salvatierra", "Altsasu/Alsasua", "Tudela"
        };

        public ObservableCollection<string> SugerenciasProvincias { get; } = new()
        {
            "Gipuzkoa", "Bizkaia", "Araba", "Navarra", "Lapurdi", "Cantabria", "Burgos"
        };

        public ObservableCollection<string> SugerenciasVias { get; } = new()
        {
            "AP-8", "AP-1", "AP-68", "N-1", "A-15", "A-1", "N-634", "N-240",
            "BI-637", "BI-30", "BI-631", "BI-636", "N-637",
            "GI-20", "GI-636", "GI-11", "GI-40", "GI-41", "GI-2632", "GI-627",
            "N-622", "A-3002", "N-102", "A-625"
        };

        public ObservableCollection<string> IncidenceTypes { get; }
        public ObservableCollection<string> IncidenceCauses { get; }

        // --- CAMPOS CÁMARA ---
        [ObservableProperty] private string _nombreCamara = string.Empty;
        [ObservableProperty] private string _urlCamara = string.Empty;
        [ObservableProperty] private string _carreteraCamara = string.Empty;
        [ObservableProperty] private string _kmCamara = string.Empty;
        [ObservableProperty] private string _direccionCamara = "Ubicación Sistema";
        [ObservableProperty] private string _latitudCamara = "43.3400";
        [ObservableProperty] private string _longitudCamara = "-1.7900";

        // --- CAMPOS INCIDENCIA ---
        [ObservableProperty] private string _typeIncidence = string.Empty;
        [ObservableProperty] private string _incidenceLevel = "Verde";
        [ObservableProperty] private string _roadIncidence = string.Empty;
        [ObservableProperty] private string _directionIncidence = "Ambos";
        [ObservableProperty] private string _descriptionIncidence = string.Empty;

        [ObservableProperty] private string _provinceIncidence = "Gipuzkoa";
        [ObservableProperty] private string _latitudInc = "43.3400";
        [ObservableProperty] private string _longitudInc = "-1.7900";

        [ObservableProperty] private string _cityIncidence = "Irun";

        // Lógica de detección automática de provincia optimizada
        partial void OnCityIncidenceChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            var valLower = value.ToLower().Trim();

            foreach (var kvp in MapaMunicipios)
            {
                // Buscamos si el texto introducido contiene alguna de las ciudades de la lista
                if (kvp.Value.Any(c => valLower.Contains(c)))
                {
                    ProvinceIncidence = kvp.Key;
                    return; // Provincia encontrada, salimos
                }
            }
        }

        public AddItemViewModel(ITrafficService trafficService, Action closeWindow)
        {
            _trafficService = trafficService;
            _closeWindow = closeWindow;

            IncidenceTypes = new ObservableCollection<string>
            {
                "Seguridad vial", "Obras", "Meteorología", "Vialidad invernal", "Puertos de montaña"
            };

            IncidenceCauses = new ObservableCollection<string>
            {
                "Accidente", "Avería", "Retención", "Obras", "Mantenimiento",
                "Nieve", "Hielo", "Lluvia", "Niebla", "Viento", "Obstáculos"
            };
        }

        [RelayCommand]
        private void PasteCoordinates()
        {
            try
            {
                if (!Clipboard.ContainsText()) return;

                string text = Clipboard.GetText();
                // Limpieza agresiva de caracteres comunes en copias de Google Maps / GeoJSON
                text = text.Replace("Lat:", "").Replace("Lon:", "").Replace("Latitude:", "").Replace("Longitude:", "")
                           .Replace("(", "").Replace(")", "").Replace("[", "").Replace("]", "").Trim();

                // Separadores comunes: coma, espacio, tabulador
                var parts = text.Split(new[] { ',', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 2)
                {
                    string lat = parts[0].Trim();
                    string lon = parts[1].Trim();

                    if (IsCameraSelected)
                    {
                        LatitudCamara = lat;
                        LongitudCamara = lon;
                    }
                    else
                    {
                        LatitudInc = lat;
                        LongitudInc = lon;
                    }
                }
            }
            catch (Exception)
            {
                // Silencioso o log si tuvieras logger
            }
        }

        [RelayCommand] private void Close() => _closeWindow?.Invoke();
        [RelayCommand] private void SwitchMode(string mode) => IsCameraSelected = (mode == "Camera");

        private string NormalizeCoordinate(string input) => input?.Replace(',', '.') ?? "0";

        private double ParseCoordinate(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return 0.0;
            // CultureInfo.InvariantCulture asegura que el punto '.' sea el separador decimal
            return double.TryParse(NormalizeCoordinate(input), NumberStyles.Any, CultureInfo.InvariantCulture, out double result)
                ? result
                : 0.0;
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
                    if (string.IsNullOrWhiteSpace(NombreCamara))
                    {
                        MessageBox.Show("El nombre de la cámara es obligatorio.");
                        return;
                    }

                    var newCam = new Camara
                    {
                        Id = new Random().Next(2000, 9999),
                        Nombre = NombreCamara,
                        UrlImagen = string.IsNullOrWhiteSpace(UrlCamara) ? DEFAULT_CAM_URL : UrlCamara,
                        Carretera = CarreteraCamara,
                        Kilometro = KmCamara,
                        Direccion = DireccionCamara,
                        // Normalizamos a string con punto para la API
                        Latitud = NormalizeCoordinate(LatitudCamara),
                        Longitud = NormalizeCoordinate(LongitudCamara)
                    };
                    success = await _trafficService.AddCamaraAsync(newCam);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(TypeIncidence))
                    {
                        MessageBox.Show("El tipo de incidencia es obligatorio.");
                        return;
                    }

                    DateTime fechaFinal = new DateTime(
                        SelectedDate.Year, SelectedDate.Month, SelectedDate.Day,
                        SelectedTime.Hour, SelectedTime.Minute, 0);

                    var newInc = new Incidencia
                    {
                        IncidenceId = "0", // Backend genera el ID real
                        IncidenceType = TypeIncidence,
                        IncidenceLevel = IncidenceLevel,
                        Road = RoadIncidence,
                        CityTown = CityIncidence,
                        Province = ProvinceIncidence,
                        Cause = DescriptionIncidence,
                        Direction = DirectionIncidence,
                        Latitude = ParseCoordinate(LatitudInc),
                        Longitude = ParseCoordinate(LongitudInc),
                        StartDate = fechaFinal
                    };
                    success = await _trafficService.AddIncidenciaAsync(newInc);
                }

                if (success)
                {
                    await Task.Delay(300); // Feedback visual
                    Close();
                }
                else
                {
                    MessageBox.Show("Error al guardar en el servidor. Verifique la conexión.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error interno: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool CanSubmit() => !IsBusy;
    }
}