using MAVPC.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MAVPC.Services
{
    public interface ITrafficService
    {
        // Métodos de Lectura (GET)
        Task<List<Camara>> GetCamarasAsync();

        // El actual (solo activas)
        Task<List<Incidencia>> GetIncidenciasAsync();

        // --- NUEVO: El que trae todo el historial (api/incidencias) ---
        Task<List<Incidencia>> GetAllIncidenciasAsync();

        // Métodos de Escritura (POST/DELETE)
        Task<bool> AddCamaraAsync(Camara nuevaCamara);
        Task<bool> AddIncidenciaAsync(Incidencia nuevaIncidencia);
        Task<bool> DeleteCamaraAsync(string id);
    }
}

