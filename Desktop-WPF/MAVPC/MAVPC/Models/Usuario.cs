using System.Text.Json.Serialization;

namespace MAVPC.Models;

/// <summary>
/// Modelo de usuario para autenticación y perfil.
/// Mapeado contra la respuesta JSON del Backend.
/// </summary>
public class Usuario
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("usuario")]
    public string NombreUsuario { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    // ADVERTENCIA: Manejar contraseñas en texto plano en memoria es un riesgo.
    // Asegúrate de usar HTTPS en el servicio.
    [JsonPropertyName("contrasena")]
    public string Contrasena { get; set; } = string.Empty;

    [JsonPropertyName("urlImage")]
    public string? UrlImage { get; set; } // Nullable, puede no tener avatar.
}