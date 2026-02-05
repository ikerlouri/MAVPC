using System.Text.Json.Serialization;

namespace MAVPC.Models
{
    /// <summary>
    /// DTO ligero para enviar datos al mapa (Leaflet/WebView2).
    /// </summary>
    public class MapMarkerModel
    {
        // Añadido ID para poder identificar el click en el mapa
        [JsonPropertyName("Id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("Lat")]
        public double Lat { get; set; }

        [JsonPropertyName("Lon")]
        public double Lon { get; set; }

        // Tipos aceptados por el JS: "camara", "incidencia", "obra", "nieve"
        [JsonPropertyName("Type")]
        public string Type { get; set; } = "incidencia";

        [JsonPropertyName("Title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("Description")]
        public string Description { get; set; } = string.Empty;

        public object? DataObject { get; set; }
    }
}