using MAVPC.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MAVPC.Services
{
    /// <summary>
    /// Interfaz para la gestión de datos de tráfico (Cámaras e Incidencias).
    /// </summary>
    public interface ITrafficService
    {
        // --- LECTURA (GET) ---

        /// <summary>
        /// Obtiene la lista de cámaras disponibles.
        /// </summary>
        Task<List<Camara>> GetCamarasAsync();

        /// <summary>
        /// Obtiene SOLO las incidencias que están activas actualmente (endpoint /listarActual).
        /// Usado principalmente por el Mapa.
        /// </summary>
        Task<List<Incidencia>> GetIncidenciasAsync();

        /// <summary>
        /// Obtiene TODO el historial de incidencias (endpoint /incidencias).
        /// Usado para generar reportes y estadísticas en el Dashboard.
        /// </summary>
        Task<List<Incidencia>> GetAllIncidenciasAsync();

        // --- ESCRITURA (POST/DELETE) ---

        /// <summary>
        /// Registra una nueva cámara en el sistema.
        /// </summary>
        Task<bool> AddCamaraAsync(Camara nuevaCamara);

        /// <summary>
        /// Registra una nueva incidencia.
        /// </summary>
        Task<bool> AddIncidenciaAsync(Incidencia nuevaIncidencia);

        /// <summary>
        /// Elimina una cámara del sistema por su ID.
        /// </summary>
        Task<bool> DeleteCamaraAsync(int id);
    }
}