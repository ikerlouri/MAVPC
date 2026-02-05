using System;

namespace MAVPC.Models
{
    public class TrafficPoint
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "Cámara"; // Cámara, Sensor, Incidencia
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Status { get; set; } = "Fluido"; // Fluido, Denso, Retención, Cerrado
        public DateTime LastUpdate { get; set; } = DateTime.Now;

        // --- PROPIEDADES VISUALES (Helpers para la vista) ---

        // Icono según tipo
        public string IconKind
        {
            get
            {
                return Type switch
                {
                    "Cámara" => "Cctv",
                    "Sensor" => "AccessPoint",
                    "Incidencia" => "AlertCircle",
                    _ => "MapMarker"
                };
            }
        }

        // Color según estado
        public string StatusColor
        {
            get
            {
                return Status switch
                {
                    "Fluido" => "#00FF00",       // Verde
                    "Denso" => "#FFA500",        // Naranja
                    "Retención" => "#FF0000",    // Rojo
                    "Cerrado" => "#808080",      // Gris
                    _ => "#FFFFFF"
                };
            }
        }
    }
}

