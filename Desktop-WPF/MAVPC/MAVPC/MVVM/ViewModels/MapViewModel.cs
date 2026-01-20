using CommunityToolkit.Mvvm.ComponentModel;
using MAVPC.Models;
using MAVPC.Services;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using MAVPC.Utils;
using System.Text.Json; // Necesario para reconvertir el DataObject
using System.Windows;   // Necesario para abrir ventanas (Application.Current)

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

        // --- NUEVO COMANDO: GESTIONA EL CLICK EN EL MAPA ---
        [RelayCommand]
        public void MarkerClicked(MapMarkerModel item)
        {
            if (item == null || item.DataObject == null) return;

            try
            {
                // Usamos el Dispatcher para asegurar que la ventana se abre en el hilo de la UI
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // 1. SI ES CÁMARA
                    if (item.Type.ToLower().Contains("camara"))
                    {
                        // El DataObject llega como un JsonElement genérico, hay que convertirlo a Camara
                        var jsonString = item.DataObject.ToString();
                        // Importante: CaseInsensitive por si las moscas
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var camara = JsonSerializer.Deserialize<Camara>(jsonString, options);

                        if (camara != null)
                        {
                            var win = new CameraWindow(camara);
                            win.Show();
                        }
                    }
                    // 2. SI ES INCIDENCIA (Obra, Nieve, Accidente...)
                    else
                    {
                        var jsonString = item.DataObject.ToString();
                        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var incidencia = JsonSerializer.Deserialize<Incidencia>(jsonString, options);

                        if (incidencia != null)
                        {
                            var win = new IncidentWindow(incidencia);
                            win.Show();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error abriendo ventana de detalle: {ex.Message}");
            }
        }

        public async Task CargarDatosReales()
        {
            try
            {
                var incidencias = await _trafficService.GetIncidenciasAsync();
                var camaras = await _trafficService.GetCamarasAsync();

                var listaFinal = new ObservableCollection<MapMarkerModel>();

                // 1. INCIDENCIAS
                if (incidencias != null)
                {
                    foreach (var item in incidencias)
                    {
                        if (item.Latitude == null || item.Longitude == null) continue;
                        if (item.Latitude == 0 || item.Longitude == 0) continue;

                        double rawLat = item.Latitude.Value;
                        double rawLon = item.Longitude.Value;
                        double finalLat, finalLon;

                        if (Math.Abs(rawLat) <= 90 && Math.Abs(rawLon) <= 180)
                        {
                            finalLat = rawLat;
                            finalLon = rawLon;
                        }
                        else
                        {
                            double utmY = Math.Max(rawLat, rawLon);
                            double utmX = Math.Min(rawLat, rawLon);
                            var (cLat, cLon) = GpsUtils.UtmToLatLng(utmX, utmY, zone: 30);
                            finalLat = cLat;
                            finalLon = cLon;
                        }

                        listaFinal.Add(new MapMarkerModel
                        {
                            Lat = finalLat,
                            Lon = finalLon,
                            Type = DetectarTipo(item.IncidenceType, item.Cause),
                            Title = item.IncidenceType ?? "Incidencia",
                            Description = $"{item.Cause} - {item.CityTown}",
                            DataObject = item // Guardamos el objeto original
                        });
                    }
                }

                // 2. CÁMARAS
                if (camaras != null)
                {
                    foreach (var cam in camaras)
                    {
                        bool latOk = double.TryParse(cam.Latitud?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double rawLat);
                        bool lonOk = double.TryParse(cam.Longitud?.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double rawLon);

                        if (latOk && lonOk)
                        {
                            double finalLat, finalLon;

                            if (Math.Abs(rawLat) <= 90 && Math.Abs(rawLon) <= 180)
                            {
                                finalLat = rawLat;
                                finalLon = rawLon;
                            }
                            else
                            {
                                double utmY = Math.Max(rawLat, rawLon);
                                double utmX = Math.Min(rawLat, rawLon);
                                var (cLat, cLon) = GpsUtils.UtmToLatLng(utmX, utmY, zone: 30);
                                finalLat = cLat;
                                finalLon = cLon;
                            }

                            listaFinal.Add(new MapMarkerModel
                            {
                                Lat = finalLat,
                                Lon = finalLon,
                                Type = "camara",
                                Title = cam.Nombre ?? "Cámara DGT",
                                Description = cam.Carretera ?? "",
                                DataObject = cam // Guardamos el objeto original
                            });
                        }
                    }
                }

                Markers = listaFinal;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando datos del mapa: {ex.Message}");
            }
        }

        private string DetectarTipo(string? tipo, string? causa)
        {
            string t = (tipo ?? "").ToLower();
            string c = (causa ?? "").ToLower();

            if (t.Contains("obra") || c.Contains("obra") || c.Contains("mantenimiento")) return "obra";
            if (t.Contains("nieve") || t.Contains("hielo") || c.Contains("nieve")) return "nieve";

            return "incidencia";
        }
    }
}