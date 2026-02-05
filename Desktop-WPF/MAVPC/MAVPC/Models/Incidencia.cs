using System;
using System.Text.Json.Serialization;

namespace MAVPC.Models
{
    /// <summary>
    /// Modelo que representa una incidencia de tráfico proveniente de la API Open Data.
    /// Incluye lógica de presentación básica (Color/Icono).
    /// </summary>
    public class Incidencia
    {
        // --- PROPIEDADES RAW (Mapeo directo del JSON) ---

        [JsonPropertyName("incidenceId")]
        public string IncidenceId { get; set; } = string.Empty;

        [JsonPropertyName("incidenceType")]
        public string? IncidenceType { get; set; }

        [JsonPropertyName("incidenceLevel")]
        public string? IncidenceLevel { get; set; }

        [JsonPropertyName("road")]
        public string? Road { get; set; }

        [JsonPropertyName("cityTown")]
        public string? CityTown { get; set; }

        [JsonPropertyName("province")]
        public string? Province { get; set; }

        [JsonPropertyName("cause")]
        public string? Cause { get; set; }

        [JsonPropertyName("direction")]
        public string? Direction { get; set; }

        // Nota: Asumimos que la API devuelve WGS84 (Decimal) para incidencias.
        // Si devolviera UTM, necesitaríamos strings y procesar con GpsUtils.
        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime? StartDate { get; set; }


        // --- LÓGICA DE PRESENTACIÓN (COMPUTADA) ---

        [JsonIgnore] // No enviamos esto a la API, es solo para la UI de WPF
        public string StatusColor => DetermineColor();

        [JsonIgnore]
        public string IconKind => DetermineIcon();

        private string DetermineColor()
        {
            if (string.IsNullOrEmpty(IncidenceLevel)) return "#00D4FF"; // Cyan por defecto

            // Uso de StringComparison para rendimiento (evita crear nuevos strings con ToLower)
            if (Contains(IncidenceLevel, "rojo") || Contains(IncidenceLevel, "red")) return "#FF003C"; // Rojo Neón
            if (Contains(IncidenceLevel, "negro") || Contains(IncidenceLevel, "black")) return "#000000"; // Carretera cortada
            if (Contains(IncidenceLevel, "amarillo") || Contains(IncidenceLevel, "yellow")) return "#FFD700";
            if (Contains(IncidenceLevel, "verde") || Contains(IncidenceLevel, "green")) return "#00FF00";

            return "#00D4FF";
        }

        private string DetermineIcon()
        {
            // Unificamos tipo y causa para la búsqueda
            string type = IncidenceType ?? "";
            string cause = Cause ?? "";

            if (Contains(type, "obra") || Contains(cause, "obra") || Contains(cause, "mantenimiento"))
                return "Cone"; // Icono de cono

            if (Contains(type, "accidente") || Contains(cause, "vuelco") || Contains(cause, "choque") || Contains(cause, "alcance"))
                return "CarCrash";

            if (Contains(cause, "avería") || Contains(cause, "averia"))
                return "CarWrench";

            if (Contains(cause, "gasoil") || Contains(cause, "aceite"))
                return "Oil";

            if (Contains(type, "meteo") || Contains(cause, "nieve") || Contains(cause, "lluvia") || Contains(cause, "hielo"))
                return "WeatherPouring";

            if (Contains(type, "evento"))
                return "CalendarStar";

            return "AlertCircle"; // Genérico
        }

        // Helper para búsqueda case-insensitive rápida
        private bool Contains(string source, string toCheck)
        {
            return source.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}