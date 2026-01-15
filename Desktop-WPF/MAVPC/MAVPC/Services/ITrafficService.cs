using MAVPC.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MAVPC.Services
{
    public interface ITrafficService
    {
        // Métodos de Lectura (GET)
        Task<List<Camara>> GetCamarasAsync();
        Task<List<Incidencia>> GetIncidenciasAsync();

        // Métodos de Escritura (POST)
        Task<bool> AddCamaraAsync(Camara nuevaCamara);
        Task<bool> AddIncidenciaAsync(Incidencia nuevaIncidencia);
    }
}