using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAVPC.Models;
using MAVPC.Services;
using MAVPC.Utils;
using MAVPC.MVVM.Views; // Necesario para abrir CameraWindow e IncidentWindow
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace MAVPC.MVVM.ViewModels
{
    /// <summary>
    /// ViewModel que gestiona la lógica del mapa interactivo.
    /// Combina datos de dos fuentes con formatos distintos (Incidencias y Cámaras)
    /// y gestiona la comunicación con el mapa web.
    /// </summary>
    public partial class MapViewModel : ObservableObject
    {
        private readonly ITrafficService _trafficService;

        /// <summary>
        /// Colección de marcadores que se mostrarán en el mapa.
        /// </summary>
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

            // Carga inicial asíncrona ("Fire and Forget")
            _ = CargarDatosReales();
        }

        /// <summary>
        /// Carga datos desde el servicio, normaliza coordenadas (UTM/Decimal) y unifica formatos.
        /// </summary>
        public async Task CargarDatosReales()
        {
            var listaTemp = new ObservableCollection<MapMarkerModel>();

            // --- BLOQUE 1: INCIDENCIAS (API NUEVA - Formato Double) ---
            try
            {
                var incidencias = await _trafficService.GetIncidenciasAsync();
                if (incidencias != null)
                {
                    foreach (var item in incidencias)
                    {
                        // Validación de integridad
                        if (item.Latitude == null || item.Longitude == null) continue;
                        if (item.Latitude == 0 && item.Longitude == 0) continue;

                        // En la API nueva, vienen directamente como Double
                        double lat = item.Latitude.Value;
                        double lon = item.Longitude.Value;

                        // Conversión UTM si es necesario (cuando las coordenadas son valores muy altos)
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
                            DataObject = item // Guardamos el objeto original para el click
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error Incidencias: {ex.Message}");
            }

            // --- BLOQUE 2: CÁMARAS (API VIEJA - Formato String) ---
            try
            {
                var camaras = await _trafficService.GetCamarasAsync();
                if (camaras != null)
                {
                    foreach (var cam in camaras)
                    {
                        // Parseo manual: La API vieja envía Strings con comas o puntos.
                        // Reemplazamos coma por punto para asegurar el parseo InvariantCulture.
                        string sLat = cam.Latitud?.Replace(",", ".") ?? "0";
                        string sLon = cam.Longitud?.Replace(",", ".") ?? "0";

                        if (double.TryParse(sLat, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat) &&
                            double.TryParse(sLon, NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
                        {
                            if (lat == 0 && lon == 0) continue;

                            // Verificación UTM también para cámaras
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
                                Type = "camara", // Tipo fijo para lógica de iconos
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
            // Usamos el Dispatcher para asegurar que la UI se actualiza en el hilo principal
            Application.Current.Dispatcher.Invoke(() =>
            {
                Markers = listaTemp;
            });
        }

        /// <summary>
        /// Comando ejecutado al hacer clic en un marcador del mapa web.
        /// Abre la ventana emergente correspondiente según el tipo de dato.
        /// </summary>
        [RelayCommand]
        public void MarkerClicked(MapMarkerModel item)
        {
            if (item?.DataObject == null) return;

            try
            {
                // Reconstruir objeto desde el JsonElement o string que devuelve el DataObject genérico
                // Esto es necesario porque al pasar por el puente de WebView2, el objeto se serializa
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

        /// <summary>
        /// Determina la clase CSS o tipo de icono basándose en las descripciones de texto.
        /// </summary>
        private string DetectarTipo(string? tipo, string? causa)
        {
            string t = (tipo ?? "").ToLower();
            string c = (causa ?? "").ToLower();

            // 1. OBRAS
            if (t.Contains("obra") || c.Contains("obra") || c.Contains("mantenimiento"))
                return "obra";

            // 2. METEOROLOGÍA
            if (t.Contains("nieve") || c.Contains("nieve") ||
                t.Contains("hielo") || c.Contains("hielo") ||
                t.Contains("invernal") ||           // "Vialidad invernal tramos"
                t.Contains("montaña") ||            // "Puertos de montaña"
                t.Contains("lluvia") || c.Contains("lluvia") ||
                t.Contains("viento") ||
                t.Contains("niebla") ||
                t.Contains("meteo"))
                return "meteo";

            // 3. CÁMARAS
            if (t.Contains("camara")) return "camara";

            // 4. RESTO (Accidentes, Seguridad vial, etc.)
            return "incidencia";
        }
    }
}