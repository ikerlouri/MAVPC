using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAVPC.Services;
using MAVPC.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MAVPC.MVVM.ViewModels
{
    /// <summary>
    /// ViewModel para la gestión de usuarios.
    /// Incluye filtrado en tiempo real, hashing de contraseñas y operaciones CRUD completas.
    /// </summary>
    public partial class UsersViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        // Vista de colección para aplicar filtros sin modificar la lista original
        private ICollectionView _usersCollectionView;

        [ObservableProperty] private ObservableCollection<Usuario> _usuarios;
        [ObservableProperty] private Usuario? _selectedUsuario;

        #region Propiedades del Diálogo (Modal)

        [ObservableProperty] private bool _isDialogOpen;
        [ObservableProperty] private string _dialogTitle;

        // Variables de estado para saber si estamos creando o editando
        private bool _isEditMode;

        #endregion

        #region Propiedades del Formulario

        [ObservableProperty] private string _formUsername;
        [ObservableProperty] private string _formEmail;
        [ObservableProperty] private string _formUrlImage;

        #endregion

        #region Buscador y Filtro

        private string _searchText;
        /// <summary>
        /// Texto de búsqueda. Al cambiar, actualiza automáticamente el filtro de la lista.
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    // Refrescamos la vista para aplicar el filtro de nuevo
                    _usersCollectionView?.Refresh();
                }
            }
        }

        #endregion

        public UsersViewModel(IAuthService authService)
        {
            _authService = authService;
            Usuarios = new ObservableCollection<Usuario>();

            // Carga inicial de datos
            LoadUsers();
        }

        /// <summary>
        /// Carga los usuarios desde la API y configura el ICollectionView para el filtrado.
        /// </summary>
        private async void LoadUsers()
        {
            try
            {
                var listaApi = await _authService.GetUsuariosAsync();

                Usuarios.Clear();
                foreach (var user in listaApi)
                {
                    Usuarios.Add(user);
                }

                // --- CONFIGURACIÓN DEL FILTRO ---
                // Enlazamos la CollectionView a la ObservableCollection
                _usersCollectionView = CollectionViewSource.GetDefaultView(Usuarios);

                // Definimos el predicado de filtrado
                _usersCollectionView.Filter = (obj) =>
                {
                    if (string.IsNullOrEmpty(SearchText)) return true;

                    if (obj is Usuario user)
                    {
                        string search = SearchText.ToLower();
                        // Filtramos por Nombre de Usuario O Email
                        return (user.NombreUsuario != null && user.NombreUsuario.ToLower().Contains(search)) ||
                               (user.Email != null && user.Email.ToLower().Contains(search));
                    }
                    return false;
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar usuarios: {ex.Message}");
            }
        }

        /// <summary>
        /// Genera un hash SHA256 de la contraseña para no enviarla en texto plano.
        /// </summary>
        private string HashPassword(string rawPassword)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawPassword));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        #region Comandos del Diálogo

        [RelayCommand]
        private void OpenCreateDialog()
        {
            FormUsername = "";
            FormEmail = "";
            FormUrlImage = "";
            _isEditMode = false;
            DialogTitle = "NUEVO USUARIO";
            IsDialogOpen = true;
        }

        [RelayCommand]
        private void OpenEditDialog(Usuario? usuarioToEdit)
        {
            if (usuarioToEdit == null) return;

            SelectedUsuario = usuarioToEdit;
            FormUsername = usuarioToEdit.NombreUsuario;
            FormEmail = usuarioToEdit.Email;
            FormUrlImage = usuarioToEdit.UrlImage;

            _isEditMode = true;
            DialogTitle = $"EDITAR: {usuarioToEdit.NombreUsuario.ToUpper()}";
            IsDialogOpen = true;
        }

        [RelayCommand]
        private void CloseDialog()
        {
            IsDialogOpen = false;
        }

        #endregion

        #region Comandos CRUD

        [RelayCommand]
        private async Task DeleteUser(Usuario? usuarioToDelete)
        {
            if (usuarioToDelete == null) return;

            var result = MessageBox.Show($"¿Eliminar a {usuarioToDelete.NombreUsuario}?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                bool exito = await _authService.EliminarUsuarioAsync(usuarioToDelete.Id);

                if (exito)
                {
                    Usuarios.Remove(usuarioToDelete);
                }
                else
                {
                    MessageBox.Show("No se pudo eliminar el usuario (API Error).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Guarda (Crea o Edita) el usuario, gestionando el hash de la contraseña si es necesario.
        /// </summary>
        /// <param name="parameter">El PasswordBox de la vista para obtener la contraseña de forma segura.</param>
        [RelayCommand]
        private async Task Save(object parameter)
        {
            var passwordBox = parameter as PasswordBox;
            var rawPassword = passwordBox?.Password ?? "";

            if (string.IsNullOrWhiteSpace(FormUsername) || string.IsNullOrWhiteSpace(FormEmail))
            {
                MessageBox.Show("El nombre y el email son obligatorios.", "Faltan datos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // === CASO EDICIÓN ===
                if (_isEditMode && SelectedUsuario != null)
                {
                    string? passwordToSend = null;

                    // Solo hasheamos si el usuario escribió una nueva contraseña
                    if (!string.IsNullOrWhiteSpace(rawPassword))
                    {
                        passwordToSend = HashPassword(rawPassword);
                    }

                    var usuarioActualizado = new Usuario
                    {
                        Id = SelectedUsuario.Id,
                        NombreUsuario = FormUsername,
                        Email = FormEmail,
                        UrlImage = FormUrlImage ?? SelectedUsuario.UrlImage,
                        // Si no hay password nueva, mantenemos la vieja
                        Contrasena = passwordToSend ?? SelectedUsuario.Contrasena
                    };

                    bool exito = await _authService.EditarUsuarioAsync(usuarioActualizado);

                    if (exito)
                    {
                        // Actualizamos la lista localmente para reflejar cambios sin recargar todo
                        var index = Usuarios.IndexOf(SelectedUsuario);
                        if (index >= 0) Usuarios[index] = usuarioActualizado;

                        MessageBox.Show("Usuario actualizado correctamente.");
                        CloseDialog();
                        if (passwordBox != null) passwordBox.Password = "";
                    }
                    else
                    {
                        MessageBox.Show("Error al actualizar en el servidor.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                // === CASO CREACIÓN ===
                else
                {
                    if (string.IsNullOrWhiteSpace(rawPassword))
                    {
                        MessageBox.Show("La contraseña es obligatoria para nuevos usuarios.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string hashedPassword = HashPassword(rawPassword);

                    var newUsuario = new Usuario
                    {
                        Id = 0, // La API asignará el ID real
                        NombreUsuario = FormUsername,
                        Email = FormEmail,
                        Contrasena = hashedPassword,
                        UrlImage = FormUrlImage ?? ""
                    };

                    bool exito = await _authService.CrearUsuarioAsync(newUsuario);

                    if (exito)
                    {
                        MessageBox.Show("Usuario creado correctamente.");
                        CloseDialog();
                        if (passwordBox != null) passwordBox.Password = "";
                        LoadUsers(); // Recargamos para obtener el ID real generado por la BD
                    }
                    else
                    {
                        MessageBox.Show("Error al crear usuario en el servidor.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}");
            }
        }

        #endregion
    }
}