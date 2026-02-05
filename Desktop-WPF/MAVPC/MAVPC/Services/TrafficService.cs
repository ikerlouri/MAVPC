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
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
        }

        // --- MÉTODOS EXISTENTES ---

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
            // Este llama a /listarActual (Solo lo que está pasando AHORA)
            return await FetchIncidencias($"{BASE_URL}incidencias/listarActual");
        }

        // --- NUEVO MÉTODO: HISTORIAL COMPLETO ---
        public async Task<List<Incidencia>> GetAllIncidenciasAsync()
        {
            // Este llama a /incidencias (Histórico completo para reportes)
            return await FetchIncidencias($"{BASE_URL}incidencias");
        }

        // He refactorizado la lógica de fetch para no repetir código, 
        // ya que ambos endpoints devuelven la misma estructura JSON.
        private async Task<List<Incidencia>> FetchIncidencias(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                var jsonString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Error HTTP: {response.StatusCode}");
                    return new List<Incidencia>();
                }

                var lista = JsonSerializer.Deserialize<List<Incidencia>>(jsonString, _jsonOptions);

                if (lista != null)
                {
                    // Filtramos latitud 0 o nula para evitar basura en el mapa/reporte
                    return lista.FindAll(x => x.Latitude != null && x.Latitude != 0);
                }

                return new List<Incidencia>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR FETCH: {ex.Message}");
                return new List<Incidencia>();
            }
        }

        // --- MÉTODOS DE ESCRITURA ---

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

