using System.Text.Json.Serialization;

namespace MAVPC.Models
{
    public class Camara
    {
        // Mapeamos "cameraId" del JSON a nuestra propiedad Id
        [JsonPropertyName("cameraId")]
        public string Id { get; set; } = string.Empty;

        // Mapeamos "cameraName" -> Nombre
        [JsonPropertyName("cameraName")]
        public string Nombre { get; set; } = string.Empty;

        // Mapeamos "urlImage" -> UrlImagen
        // El '?' es importante porque en tu JSON a veces viene null
        [JsonPropertyName("urlImage")]
        public string? UrlImagen { get; set; }

        [JsonPropertyName("road")]
        public string Carretera { get; set; } = string.Empty;

        [JsonPropertyName("kilometer")]
        public string Kilometro { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public string Direccion { get; set; } = string.Empty;

        // Nota: Tus coordenadas vienen en formato UTM (números gigantes), 
        // no en latitud/longitud normal. Las guardamos como string por ahora.
        [JsonPropertyName("latitude")]
        public string Latitud { get; set; } = string.Empty;

        [JsonPropertyName("longitude")]
        public string Longitud { get; set; } = string.Empty;
    }
}
============================================================
ARCHIVO: C:\Users\2dam3\Documents\Retos\MAVPC\Desktop-WPF\MAVPC\MAVPC\Models\Camara.cs
============================================================
