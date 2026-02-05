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
using System.Windows.Data;
using System.Windows.Media;

namespace MAVPC.MVVM.ViewModels
{
    public partial class AddItemViewModel : ObservableObject
    {
        private readonly ITrafficService _trafficService;
        private readonly Action _closeWindow;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private bool _isBusy;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentColor))]
        [NotifyPropertyChangedFor(nameof(IsCameraMode))]
        private bool _isCameraSelected = true;

        public bool IsCameraMode => IsCameraSelected;

        public Brush CurrentColor => IsCameraSelected
            ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FFFF"))
            : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF007F"));

        // --- FECHA Y HORA ---
        [ObservableProperty] private DateTime _selectedDate = DateTime.Now;
        [ObservableProperty] private DateTime _selectedTime = DateTime.Now;

        // --- LISTAS DE AUTOCOMPLETADO (CIUDADES) ---
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

        // --- MEJORA: LISTA DE VÍAS COMPARTIDA (RECICLAJE DE LÓGICA) ---
        // Se usa tanto para Cámaras como para Incidencias
        public ObservableCollection<string> SugerenciasVias { get; } = new()
        {
            // Principales
            "AP-8", "AP-1", "AP-68", "N-1", "A-15", "A-1", "N-634", "N-240",
            // Bizkaia
            "BI-637", "BI-30", "BI-631", "BI-636", "N-637", 
            // Gipuzkoa
            "GI-20", "GI-636", "GI-11", "GI-40", "GI-41", "GI-2632", "GI-627",
            // Araba
            "N-622", "A-3002", "N-102", "A-625"
        };

        // --- LISTAS DE TIPOS ---
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

        [ObservableProperty] private string _cityIncidence = "Irun";
        partial void OnCityIncidenceChanged(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            var val = value.ToLower().Trim();

            if (new[] { "bilbao", "barakaldo", "getxo", "portugalete", "santurtzi", "basauri", "leioa", "galdakao", "durango", "sestao", "erandio", "amorebieta", "gernika", "bermeo", "mungia", "sopela", "arrigorriaga", "trapagaran", "etxebarri", "abanto", "ondarroa", "ortuella", "balmaseda", "muskiz", "lekeitio", "berriz", "güeñes", "derio", "gorliz", "orozko", "zalla", "elorrio", "lemoa", "plentzia" }.Any(c => val.Contains(c)))
                ProvinceIncidence = "Bizkaia";
            else if (new[] { "donostia", "san sebastián", "irun", "errenteria", "eibar", "zarautz", "arrasate", "mondragón", "hernani", "tolosa", "lasarte", "pasaia", "bergara", "azpeitia", "beasain", "andoain", "oñati", "zumarraga", "hondarribia", "elgoibar", "oiartzun", "azkoitia", "lazkao", "mutriku", "urniet", "deba", "zumaia", "lezo", "villabona", "astigarraga", "aretxabaleta", "oria", "getaria", "orio" }.Any(c => val.Contains(c)))
                ProvinceIncidence = "Gipuzkoa";
            else if (new[] { "vitoria", "gasteiz", "laudio", "llodio", "amurrio", "salvatierra", "agurain", "oion", "oyón", "iruña de oca", "ayala", "aiara", "zuia", "legutio", "aramaio", "laguardia", "labastida" }.Any(c => val.Contains(c)))
                ProvinceIncidence = "Araba";
            else if (new[] { "pamplona", "iruña", "tudela", "barañáin", "burlada", "egüés", "estella", "lizarra", "tafalla", "alsasua", "altsasu", "baztan", "corella", "noáin", "cintruénigo", "bera", "lesaka", "leitza", "etxarri" }.Any(c => val.Contains(c)))
                ProvinceIncidence = "Navarra";
            else if (val.Contains("hendaya") || val.Contains("hendaia") || val.Contains("bayonne") || val.Contains("baiona"))
                ProvinceIncidence = "Lapurdi";
        }

        [ObservableProperty] private string _provinceIncidence = "Gipuzkoa";
        [ObservableProperty] private string _latitudInc = "43.3400";
        [ObservableProperty] private string _longitudInc = "-1.7900";

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
                if (Clipboard.ContainsText())
                {
                    string text = Clipboard.GetText();
                    text = text.Replace("Lat:", "").Replace("Lon:", "").Replace("(", "").Replace(")", "").Trim();
                    var parts = text.Contains(',') ? text.Split(',') : text.Split(' ');

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
            }
            catch { }
        }

        [RelayCommand] private void Close() => _closeWindow?.Invoke();
        [RelayCommand] private void SwitchMode(string mode) => IsCameraSelected = (mode == "Camera");

        private string NormalizeCoordinate(string input) => input?.Replace(',', '.') ?? "0";

        private double ParseCoordinate(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return 0.0;
            if (double.TryParse(NormalizeCoordinate(input), NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
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
                        Id = "CAM-" + new Random().Next(2000, 9999).ToString(),
                        Nombre = NombreCamara,
                        UrlImagen = string.IsNullOrWhiteSpace(UrlCamara) ? "http://10.10.16.93:8080/images/default.jpg" : UrlCamara,
                        Carretera = CarreteraCamara,
                        Kilometro = KmCamara,
                        Direccion = DireccionCamara,
                        Latitud = NormalizeCoordinate(LatitudCamara),
                        Longitud = NormalizeCoordinate(LongitudCamara)
                    };
                    success = await _trafficService.AddCamaraAsync(newCam);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(TypeIncidence)) { MessageBox.Show("Tipo obligatorio"); return; }

                    DateTime fechaFinal = new DateTime(
                        SelectedDate.Year, SelectedDate.Month, SelectedDate.Day,
                        SelectedTime.Hour, SelectedTime.Minute, 0);

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
                        StartDate = fechaFinal
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

    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is bool b ? !b : value;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is bool b ? !b : value;
    }
}