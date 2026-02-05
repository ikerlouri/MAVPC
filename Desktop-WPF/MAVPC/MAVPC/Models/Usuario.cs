using System.Text.Json.Serialization;

namespace MAVPC.Models
{
    public class Usuario
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("usuario")]
        public string NombreUsuario { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("contrasena")]
        public string Contrasena { get; set; }

        [JsonPropertyName("urlImage")]
        public string UrlImage { get; set; }
    }
}
============================================================
ARCHIVO: C:\Users\2dam3\Documents\Retos\MAVPC\Desktop-WPF\MAVPC\MAVPC\Models\Usuario.cs
============================================================
