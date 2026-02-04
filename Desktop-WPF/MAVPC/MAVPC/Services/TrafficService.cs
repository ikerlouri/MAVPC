using MAVPC.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MAVPC.Services
{
    public class TrafficService : ITrafficService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        private const string BASE_URL = "https://mavpc.up.railway.app/api/";

        public TrafficService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                // IMPORTANTE: Esto permite que "incidenceId": "12345" se lea como int 12345
                // sin que tu modelo tenga que cambiar a string.
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
        }

        public async Task<List<Camara>> GetCamarasAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<Camara>>($"{BASE_URL}camaras", _jsonOptions) ?? new List<Camara>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetCamaras: {ex.Message}");
                return new List<Camara>();
            }
        }

        public async Task<List<Incidencia>> GetIncidenciasAsync()
        {
            try
            {
                string url = $"{BASE_URL}incidencias/listarActual";

                // 1. Descargamos el texto crudo para ver si llega algo
                var response = await _httpClient.GetAsync(url);
                var jsonString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Error HTTP: {response.StatusCode} - {jsonString}");
                    return new List<Incidencia>();
                }

                // 2. Configuramos las opciones A MANO aquí mismo para asegurar que traga con el ID string -> int
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString, // CLAVE: Permite leer "123" como 123
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                // 3. Intentamos convertir
                var lista = JsonSerializer.Deserialize<List<Incidencia>>(jsonString, options);

                if (lista != null)
                {
                    // Filtro para quitar las que tienen latitud 0 (que vienen muchas en tu JSON)
                    var filtradas = lista.FindAll(x => x.Latitude != null && x.Latitude != 0);
                    System.Diagnostics.Debug.WriteLine($"Incidencias cargadas: {filtradas.Count} (Originales: {lista.Count})");
                    return filtradas;
                }

                return new List<Incidencia>();
            }
            catch (Exception ex)
            {
                // --- AQUÍ VERÁS EL ERROR REAL ---
                System.Diagnostics.Debug.WriteLine("--------------------------------------------------");
                System.Diagnostics.Debug.WriteLine($"ERROR DESERIALIZANDO INCIDENCIAS: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"INNER ERROR: {ex.InnerException.Message}");
                }
                System.Diagnostics.Debug.WriteLine("--------------------------------------------------");

                return new List<Incidencia>();
            }
        }

        // --- MÉTODOS DE ESCRITURA (Sin cambios de lógica) ---

        public async Task<bool> AddCamaraAsync(Camara nuevaCamara)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BASE_URL}camaras", nuevaCamara, _jsonOptions);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> AddIncidenciaAsync(Incidencia nuevaIncidencia)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{BASE_URL}incidencias", nuevaIncidencia, _jsonOptions);
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        public async Task<bool> DeleteCamaraAsync(string id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{BASE_URL}camaras?id={id}");
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }
    }
}