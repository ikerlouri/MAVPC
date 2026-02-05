using MAVPC.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MAVPC.Services
{
    public interface IAuthService
    {
        // Login LOCAL (Solo comprueba si eres el admin de la app de escritorio)
        bool Login(string username, string password);

        // GESTIÓN DE USUARIOS DE LA API (CRUD para la app móvil)
        Task<List<Usuario>> GetUsuariosAsync();
        Task<bool> CrearUsuarioAsync(Usuario usuario);
        Task<bool> EditarUsuarioAsync(Usuario usuario);
        Task<bool> EliminarUsuarioAsync(int id);
    }
}

