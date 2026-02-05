using System;
using System.Text.Json.Serialization;

namespace MAVPC.Models
{
    public class Incidencia
    {
        // --- PROPIEDADES RAW (Coinciden 100% con tu JSON) ---

        [JsonPropertyName("incidenceId")]
        public string IncidenceId { get; set; } // CRÍTICO: En el JSON viene con comillas "", es string.

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


        // --- HELPERS VISUALES (Esto se mantiene igual, es lógica tuya) ---

        public string StatusColor
        {
            get
            {
                // Protección contra nulos
                var level = IncidenceLevel?.ToLower() ?? "";

                if (level.Contains("rojo") || level.Contains("red")) return "#FF003C"; // Rojo Neón
                if (level.Contains("negro") || level.Contains("black")) return "#000000";
                if (level.Contains("amarillo") || level.Contains("yellow")) return "#FFD700";
                if (level.Contains("verde") || level.Contains("green")) return "#00FF00";

                // Por defecto (blanco o desconocido) -> Cyan
                return "#00D4FF";
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
                if (t.Contains("meteo") || c.Contains("nieve") || c.Contains("lluvia") || c.Contains("hielo")) return "WeatherPouring";
                if (t.Contains("evento")) return "CalendarStar";

                // Seguridad vial o genérico
                return "AlertCircle";
            }
        }
    }
}
============================================================
ARCHIVO: C:\Users\2dam3\Documents\Retos\MAVPC\Desktop-WPF\MAVPC\MAVPC\Models\Incidencia.cs
============================================================
