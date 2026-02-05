using MAVPC.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MAVPC.Services
{
    /// <summary>
    /// Servicio encargado de la comunicación con la API de Usuarios alojada en Railway.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;

        // URL de producción (API en la nube)
        private const string BaseUrl = "https://mavpc.up.railway.app/api/";

        public AuthService()
        {
            _httpClient = new HttpClient();
            // Timeout preventivo de 10s para evitar congelamientos si la API duerme (cold start)
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        // --- 1. LOGIN LOCAL (Escritorio) ---
        public bool Login(string username, string password)
        {
            // Verificación simple para el administrador de la consola de escritorio
            // TODO: En el futuro esto podría validar contra la base de datos si fuera necesario
            return username == "admin" && password == "admin";
        }

        // --- 2. GESTIÓN DE USUARIOS (CRUD hacia la API) ---

        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync(BaseUrl + "usuarios");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return JsonSerializer.Deserialize<List<Usuario>>(json, options) ?? new List<Usuario>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error obteniendo usuarios: {ex.Message}");
            }
            return new List<Usuario>();
        }

        public async Task<bool> CrearUsuarioAsync(Usuario usuario)
        {
            try
            {
                var json = JsonSerializer.Serialize(usuario);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // POST a /usuarios
                var response = await _httpClient.PostAsync(BaseUrl + "usuarios", content);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> EditarUsuarioAsync(Usuario usuario)
        {
            try
            {
                var json = JsonSerializer.Serialize(usuario);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // PUT a /usuarios
                var response = await _httpClient.PutAsync(BaseUrl + "usuarios", content);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> EliminarUsuarioAsync(int id)
        {
            try
            {
                // DELETE a /usuarios?idUsuario=X
                // Nota: Mantenemos el query parameter exacto como lo requiere tu API
                string url = $"{BaseUrl}usuarios?idUsuario={id}";
                var response = await _httpClient.DeleteAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }
}