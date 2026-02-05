using MAVPC.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MAVPC.Services
{
    /// <summary>
    /// Interfaz para el servicio de identidad y gestión de usuarios.
    /// Separa el login local (app escritorio) de la gestión de usuarios remota (API).
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Valida las credenciales del administrador localmente.
        /// </summary>
        bool Login(string username, string password);

        // --- GESTIÓN DE USUARIOS DE LA API (CRUD) ---

        /// <summary>Obtiene la lista de usuarios registrados en el servidor.</summary>
        Task<List<Usuario>> GetUsuariosAsync();

        /// <summary>Registra un nuevo usuario en la base de datos.</summary>
        Task<bool> CrearUsuarioAsync(Usuario usuario);

        /// <summary>Actualiza los datos de un usuario existente.</summary>
        Task<bool> EditarUsuarioAsync(Usuario usuario);

        /// <summary>Elimina un usuario por su ID.</summary>
        Task<bool> EliminarUsuarioAsync(int id);
    }
}