namespace MAVPC.Models;

/// <summary>
/// Modelo auxiliar para los resultados del buscador (AutoSuggestBox).
/// Actúa como un DTO intermedio entre la búsqueda y la navegación.
/// </summary>
public class SearchItem
{
    public string Titulo { get; set; } = string.Empty;
    public string Subtitulo { get; set; } = string.Empty;

    // Icono Material Design por defecto
    public string Icono { get; set; } = "Magnify";

    // Color en formato Hex o nombre (WPF lo convierte automáticamente)
    public string Color { get; set; } = "White";

    // Coordenadas destino
    public double Lat { get; set; }
    public double Lon { get; set; }

    // Referencia al objeto de origen (Camara, Incidencia, etc.) para lógica avanzada
    public object? DataObject { get; set; }
}