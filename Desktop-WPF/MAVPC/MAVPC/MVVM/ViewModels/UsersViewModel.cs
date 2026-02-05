using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MAVPC.Services;
using MAVPC.Models;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.ComponentModel; // NECESARIO PARA EL FILTRO
using System.Windows.Data;   // NECESARIO PARA EL FILTRO

namespace MAVPC.MVVM.ViewModels
{
    public partial class UsersViewModel : ObservableObject
    {
        private readonly IAuthService _authService;

        // Variable para controlar la vista filtrada (NO es una propiedad observable)
        private ICollectionView _usersCollectionView;

        [ObservableProperty] private ObservableCollection<Usuario> _usuarios;
        [ObservableProperty] private Usuario? _selectedUsuario;

        // DIÁLOGO
        [ObservableProperty] private bool _isDialogOpen;
        [ObservableProperty] private string _dialogTitle;

        // FORMULARIO
        [ObservableProperty] private string _formUsername;
        [ObservableProperty] private string _formEmail;
        [ObservableProperty] private string _formUrlImage; // Añadido por si acaso lo usas luego

        private bool _isEditMode;

        // --- PROPIEDAD DEL BUSCADOR ---
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                // Al cambiar el texto, notificamos y refrescamos el filtro
                if (SetProperty(ref _searchText, value))
                {
                    _usersCollectionView?.Refresh();
                }
            }
        }

        public UsersViewModel(IAuthService authService)
        {
            _authService = authService;
            Usuarios = new ObservableCollection<Usuario>();

            // Lanzamos la carga inicial
            LoadUsers();
        }

        // Método para cargar desde la API
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
                // Obtenemos la vista por defecto de la colección
                _usersCollectionView = CollectionViewSource.GetDefaultView(Usuarios);

                // Definimos la lógica: ¿Qué filas se muestran?
                _usersCollectionView.Filter = (obj) =>
                {
                    // 1. Si el buscador está vacío, mostramos todo
                    if (string.IsNullOrEmpty(SearchText)) return true;

                    // 2. Si el objeto es un usuario, comprobamos el texto
                    if (obj is Usuario user)
                    {
                        string search = SearchText.ToLower();
                        // Buscamos coincidencia en Nombre o Email
                        return (user.NombreUsuario != null && user.NombreUsuario.ToLower().Contains(search)) ||
                               (user.Email != null && user.Email.ToLower().Contains(search));
                    }
                    return false;
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error al cargar: {ex.Message}");
            }
        }

        // --- MÉTODO DE HASHEO ---
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

        [RelayCommand]
        private void OpenCreateDialog()
        {
            FormUsername = "";
            FormEmail = "";
            FormUrlImage = ""; // Limpiamos
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
            FormUrlImage = usuarioToEdit.UrlImage; // Cargamos

            _isEditMode = true;
            DialogTitle = $"EDITAR: {usuarioToEdit.NombreUsuario.ToUpper()}";
            IsDialogOpen = true;
        }

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
                if (_isEditMode && SelectedUsuario != null)
                {
                    // === EDICIÓN ===
                    string? passwordToSend = null;

                    if (!string.IsNullOrWhiteSpace(rawPassword))
                    {
                        passwordToSend = HashPassword(rawPassword);
                    }

                    var usuarioActualizado = new Usuario
                    {
                        Id = SelectedUsuario.Id,
                        NombreUsuario = FormUsername,
                        Email = FormEmail,
                        UrlImage = FormUrlImage ?? SelectedUsuario.UrlImage, // Usamos la nueva o mantenemos
                        Contrasena = passwordToSend ?? SelectedUsuario.Contrasena
                    };

                    bool exito = await _authService.EditarUsuarioAsync(usuarioActualizado);

                    if (exito)
                    {
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
                else
                {
                    // === CREACIÓN ===
                    if (string.IsNullOrWhiteSpace(rawPassword))
                    {
                        MessageBox.Show("La contraseña es obligatoria para nuevos usuarios.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string hashedPassword = HashPassword(rawPassword);

                    var newUsuario = new Usuario
                    {
                        Id = 0,
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
                        LoadUsers(); // Recargamos para obtener ID real
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

        [RelayCommand]
        private void CloseDialog()
        {
            IsDialogOpen = false;
        }
    }
}
============================================================
ARCHIVO: C:\Users\2dam3\Documents\Retos\MAVPC\Desktop-WPF\MAVPC\MAVPC\MVVM\ViewModels\UsersViewModel.cs
============================================================
