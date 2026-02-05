using System.Text.Json.Serialization;

namespace MAVPC.Models
{
    public class Camara
    {
        // CORRECCIÓN CRÍTICA: 
        // 1. Cambiamos "cameraId" por "id" para que coincida con tu JSON.
        // 2. Cambiamos string a int para evitar problemas de formato.
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("cameraName")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("urlImage")]
        public string? UrlImagen { get; set; }

        [JsonPropertyName("road")]
        public string Carretera { get; set; } = string.Empty;

        [JsonPropertyName("kilometer")]
        public string Kilometro { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public string Direccion { get; set; } = string.Empty;

        // Latitud/Longitud las dejamos como string por seguridad en el parseo
        [JsonPropertyName("latitude")]
        public string Latitud { get; set; } = string.Empty;

        [JsonPropertyName("longitude")]
        public string Longitud { get; set; } = string.Empty;
    }
}