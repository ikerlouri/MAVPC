using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAVPC.Models;
using MAVPC.Services;
using MAVPC.Utils;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace MAVPC.MVVM.ViewModels
{
    public partial class MapViewModel : ObservableObject
    {
        private readonly ITrafficService _trafficService;

        private ObservableCollection<MapMarkerModel> _markers;
        public ObservableCollection<MapMarkerModel> Markers
        {
            get => _markers;
            set => SetProperty(ref _markers, value);
        }

        public MapViewModel(ITrafficService trafficService)
        {
            _trafficService = trafficService;
            _markers = new ObservableCollection<MapMarkerModel>();
            _ = CargarDatosReales();
        }

        public async Task CargarDatosReales()
        {
            var listaTemp = new ObservableCollection<MapMarkerModel>();

            // --- BLOQUE 1: INCIDENCIAS (API NUEVA - Double) ---
            try
            {
                var incidencias = await _trafficService.GetIncidenciasAsync();
                if (incidencias != null)
                {
                    foreach (var item in incidencias)
                    {
                        if (item.Latitude == null || item.Longitude == null) continue;
                        if (item.Latitude == 0 && item.Longitude == 0) continue;

                        double lat = item.Latitude.Value;
                        double lon = item.Longitude.Value;

                        // Conversión UTM si es necesario (coordenadas grandes)
                        if (Math.Abs(lat) > 90 || Math.Abs(lon) > 180)
                        {
                            var (cLat, cLon) = GpsUtils.UtmToLatLng(Math.Min(lat, lon), Math.Max(lat, lon), 30);
                            lat = cLat;
                            lon = cLon;
                        }

                        listaTemp.Add(new MapMarkerModel
                        {
                            Lat = lat,
                            Lon = lon,
                            Type = DetectarTipo(item.IncidenceType, item.Cause),
                            Title = item.IncidenceType ?? "Incidencia",
                            Description = $"{item.Cause} - {item.CityTown}",
                            DataObject = item
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Incidencias: {ex.Message}");
            }

            // --- BLOQUE 2: CÁMARAS (API VIEJA - String) ---
            try
            {
                var camaras = await _trafficService.GetCamarasAsync();
                if (camaras != null)
                {
                    foreach (var cam in camaras)
                    {
                        // Parseo manual de strings con coma o punto
                        string sLat = cam.Latitud?.Replace(",", ".") ?? "0";
                        string sLon = cam.Longitud?.Replace(",", ".") ?? "0";

                        if (double.TryParse(sLat, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat) &&
                            double.TryParse(sLon, NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
                        {
                            if (lat == 0 && lon == 0) continue;

                            if (Math.Abs(lat) > 90 || Math.Abs(lon) > 180)
                            {
                                var (cLat, cLon) = GpsUtils.UtmToLatLng(Math.Min(lat, lon), Math.Max(lat, lon), 30);
                                lat = cLat;
                                lon = cLon;
                            }

                            listaTemp.Add(new MapMarkerModel
                            {
                                Lat = lat,
                                Lon = lon,
                                Type = "camara",
                                Title = cam.Nombre ?? "Cámara",
                                Description = cam.Carretera ?? "",
                                DataObject = cam
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Cámaras: {ex.Message}");
            }

            // --- ACTUALIZACIÓN FINAL EN UI ---
            Application.Current.Dispatcher.Invoke(() =>
            {
                Markers = listaTemp;
            });
        }

        [RelayCommand]
        public void MarkerClicked(MapMarkerModel item)
        {
            if (item?.DataObject == null) return;

            try
            {
                // Reconstruir objeto desde el JsonElement o string que devuelve el DataObject genérico
                var jsonString = item.DataObject.ToString();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (item.Type == "camara")
                {
                    var cam = JsonSerializer.Deserialize<Camara>(jsonString, options);
                    if (cam != null) new CameraWindow(cam).Show();
                }
                else
                {
                    var inc = JsonSerializer.Deserialize<Incidencia>(jsonString, options);
                    if (inc != null) new IncidentWindow(inc).Show();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error abriendo ventana: {ex.Message}");
            }
        }

        private string DetectarTipo(string? tipo, string? causa)
        {
            string t = (tipo ?? "").ToLower();
            string c = (causa ?? "").ToLower();
            if (t.Contains("obra") || c.Contains("obra")) return "obra";
            if (t.Contains("nieve") || c.Contains("nieve")) return "nieve";
            return "incidencia";
        }
    }
}