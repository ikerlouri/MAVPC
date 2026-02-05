using System;

namespace MAVPC.Models;

/// <summary>
/// Representa un punto de interés en el dashboard (no necesariamente en el mapa).
/// Incluye lógica de presentación para listas y estados.
/// </summary>
public class TrafficPoint
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Tipos: "Cámara", "Sensor", "Incidencia"
    public string Type { get; set; } = "Cámara";

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // Estados: "Fluido", "Denso", "Retención", "Cerrado"
    public string Status { get; set; } = "Fluido";

    public DateTime LastUpdate { get; set; } = DateTime.Now;

    // --- PROPIEDADES DE PRESENTACIÓN (VIEW HELPERS) ---
    // Estas propiedades facilitan el Binding directo en XAML sin necesidad de Converters complejos.

    public string IconKind => Type switch
    {
        "Cámara" => "Cctv",
        "Sensor" => "AccessPoint",
        "Incidencia" => "AlertCircle",
        _ => "MapMarker"
    };

    public string StatusColor => Status switch
    {
        "Fluido" => "#00FF00", // Verde Neón
        "Denso" => "#FFA500", // Naranja
        "Retención" => "#FF0000", // Rojo
        "Cerrado" => "#808080", // Gris
        _ => "#FFFFFF"
    };
}