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
    /// <summary>
    /// Servicio que comunica con la API de Tráfico.
    /// Gestiona la obtención, creación y borrado de cámaras e incidencias.
    /// </summary>
    public class TrafficService : ITrafficService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        // URL de producción (API en Railway)
        private const string BASE_URL = "https://mavpc.up.railway.app/api/";

        public TrafficService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Configuración para leer JSONs (ignora mayúsculas/minúsculas y nulos)
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
        }

        #region Métodos de Lectura (GET)

        public async Task<List<Camara>> GetCamarasAsync()
        {
            try
            {
                var resultado = await _httpClient.GetFromJsonAsync<List<Camara>>($"{BASE_URL}camaras", _jsonOptions);
                return resultado ?? new List<Camara>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error GetCamaras: {ex.Message}");
                return new List<Camara>();
            }
        }

        public async Task<List<Incidencia>> GetIncidenciasAsync()
        {
            // Llama a /listarActual (Solo lo que está pasando AHORA para el Mapa)
            return await FetchIncidencias($"{BASE_URL}incidencias/listarActual");
        }

        public async Task<List<Incidencia>> GetAllIncidenciasAsync()
        {
            // Llama a /incidencias (Histórico completo para Dashboard/Reportes)
            return await FetchIncidencias($"{BASE_URL}incidencias");
        }

        /// <summary>
        /// Método auxiliar privado para no repetir la lógica de petición y limpieza de datos.
        /// </summary>
        private async Task<List<Incidencia>> FetchIncidencias(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine($"Error HTTP FetchIncidencias: {response.StatusCode}");
                    return new List<Incidencia>();
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var lista = JsonSerializer.Deserialize<List<Incidencia>>(jsonString, _jsonOptions);

                if (lista != null)
                {
                    // Filtro de seguridad: Eliminamos incidencias sin coordenadas válidas
                    // para que no rompan el mapa ni los cálculos.
                    return lista.FindAll(x => x.Latitude != null && x.Latitude != 0);
                }

                return new List<Incidencia>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR CRÍTICO EN FETCH: {ex.Message}");
                return new List<Incidencia>();
            }
        }

        #endregion

        #region Métodos de Escritura (POST/DELETE)

        public async Task<bool> AddCamaraAsync(Camara item)
        {
            try
            {
                // Creamos un objeto anónimo SIN el campo 'id'
                var paqueteLimpio = new
                {
                    cameraName = item.Nombre,
                    urlImage = item.UrlImagen,
                    road = item.Carretera,
                    kilometer = item.Kilometro,
                    address = item.Direccion,
                    latitude = item.Latitud,
                    longitude = item.Longitud
                };

                var response = await _httpClient.PostAsJsonAsync($"{BASE_URL}camaras", paqueteLimpio, _jsonOptions);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Error API Cámaras: {error}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Excepción AddCamara: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> AddIncidenciaAsync(Incidencia item)
        {
            try
            {
                // --- CONSTRUCCIÓN MANUAL DEL OBJETO (DTO) ---
                // Creamos un objeto anónimo para tener control total sobre el formato de los datos.
                // Especialmente crítico para la fecha (startDate), que la API requiere en formato exacto.
                var paqueteParaEnviar = new
                {
                    // Forzamos formato ISO 8601 limpio (yyyy-MM-ddTHH:mm:ss)
                    startDate = $"{item.StartDate:yyyy-MM-ddTHH:mm:ss}",

                    // Mapeo directo del resto de propiedades
                    incidenceType = item.IncidenceType,
                    incidenceLevel = item.IncidenceLevel,
                    autonomousRegion = "Euskadi", // Valor por defecto fijo
                    road = item.Road,
                    cityTown = item.CityTown,
                    province = item.Province,
                    cause = item.Cause,
                    direction = item.Direction,
                    latitude = item.Latitude,
                    longitude = item.Longitude
                    // Nota: No enviamos incidenceId, la BD lo genera
                };

                var response = await _httpClient.PostAsJsonAsync($"{BASE_URL}incidencias", paqueteParaEnviar);

                if (!response.IsSuccessStatusCode)
                {
                    // Debug opcional para ver qué dice el servidor si falla
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Error Servidor al guardar: {errorMsg}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Excepción al guardar incidencia: {ex.Message}");
                return false;
            }
        }

        // En TrafficService.cs
        public async Task<bool> DeleteCamaraAsync(int id) // Cambia string por int
        {
            try
            {
                // La API espera ?id=1957
                var response = await _httpClient.DeleteAsync($"{BASE_URL}camaras?id={id}");
                return response.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        #endregion
    }
}