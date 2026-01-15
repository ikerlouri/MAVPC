using System;
using System.Text.Json.Serialization;

namespace MAVPC.Models
{
    public class Incidencia
    {
        // --- PROPIEDADES RAW (Tal cual vienen del JSON) ---

        [JsonPropertyName("incidenceId")]
        public int IncidenceId { get; set; }

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

        [JsonPropertyName("latitude")]
        public double? Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double? Longitude { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime? StartDate { get; set; }


        // --- HELPERS VISUALES (Lógica de presentación únicamente) ---

        public string StatusColor
        {
            get
            {
                var level = IncidenceLevel?.ToLower() ?? "";
                if (level.Contains("rojo") || level.Contains("red")) return "#FF003C"; // Rojo Neón
                if (level.Contains("negro") || level.Contains("black")) return "#000000";
                if (level.Contains("amarillo") || level.Contains("yellow")) return "#FFD700";
                if (level.Contains("verde") || level.Contains("green")) return "#00FF00";
                return "#00D4FF"; // Azul Cyan (Default)
            }
        }

        public string IconKind
        {
            get
            {
                var t = IncidenceType?.ToLower() ?? "";
                var c = Cause?.ToLower() ?? "";

                if (t.Contains("obra") || c.Contains("obra") || c.Contains("mantenimiento")) return "Cone";
                if (t.Contains("accidente") || c.Contains("vuelco") || c.Contains("choque") || c.Contains("alcance")) return "CarCrash";
                if (c.Contains("avería") || c.Contains("averia")) return "CarWrench";
                if (c.Contains("gasoil") || c.Contains("aceite")) return "Oil";
                if (t.Contains("meteo") || c.Contains("nieve") || c.Contains("lluvia")) return "WeatherPouring";
                if (t.Contains("evento")) return "CalendarStar";

                return "AlertCircle";
            }
        }
    }
}