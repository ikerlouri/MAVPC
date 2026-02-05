using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using MAVPC.Models;
using MAVPC.MVVM.ViewModels;
using Microsoft.Web.WebView2.Core;

namespace MAVPC.MVVM.Views
{
    /// <summary>
    /// Lógica de interacción para MapView.xaml.
    /// Gestiona la inicialización del navegador WebView2 y el puente de comunicación JavaScript <-> C#.
    /// </summary>
    public partial class MapView : UserControl
    {
        private MapViewModel? _viewModel;
        private bool _isMapLoaded = false;

        public MapView()
        {
            InitializeComponent();
            InitializeAsync();

            // Escuchamos cambios en el DataContext para enlazar eventos
            this.DataContextChanged += MapView_DataContextChanged;
        }

        /// <summary>
        /// Inicializa el entorno WebView2 y navega al archivo local HTML.
        /// </summary>
        private async void InitializeAsync()
        {
            try
            {
                // Asegura que el runtime de WebView2 está listo
                await MapBrowser.EnsureCoreWebView2Async();

                string currentDir = AppDomain.CurrentDomain.BaseDirectory;
                string mapPath = Path.Combine(currentDir, "Assets", "mapa.html");

                if (File.Exists(mapPath))
                {
                    // Suscripción a mensajes que vienen desde JavaScript
                    MapBrowser.WebMessageReceived += MapBrowser_WebMessageReceived;

                    // Navegación segura usando URI absoluta
                    MapBrowser.CoreWebView2.Navigate(new Uri(mapPath).AbsoluteUri);

                    // Evento para saber cuándo el HTML ha terminado de cargar
                    MapBrowser.NavigationCompleted += MapBrowser_NavigationCompleted;
                }
                else
                {
                    MessageBox.Show($"Error crítico: No se encuentra el archivo del mapa en: {mapPath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error iniciando WebView2: " + ex.Message);
            }
        }

        private void MapBrowser_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            _isMapLoaded = true;
            RefrescarMapa(); // Si el ViewModel ya tenía datos, los enviamos ahora que el mapa está listo
        }

        private void MapView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is MapViewModel vm)
            {
                _viewModel = vm;
                // Nos suscribimos a cambios en la lista de marcadores del ViewModel
                _viewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == "Markers")
                    {
                        RefrescarMapa();
                    }
                };
                RefrescarMapa();
            }
        }

        /// <summary>
        /// Serializa los marcadores a JSON y llama a la función JS 'CargarMarcadores'.
        /// </summary>
        private async void RefrescarMapa()
        {
            if (!_isMapLoaded || _viewModel == null || _viewModel.Markers == null || MapBrowser.CoreWebView2 == null) return;

            try
            {
                var options = new JsonSerializerOptions { PropertyNamingPolicy = null };
                var json = JsonSerializer.Serialize(_viewModel.Markers, options);

                // IMPORTANTE: Escapar comillas simples para evitar errores de sintaxis en la inyección JS
                var jsonSafe = json.Replace("'", "\\'");

                // Inyección de script: Llamamos a la función global definida en mapa.html
                await MapBrowser.CoreWebView2.ExecuteScriptAsync($"CargarMarcadores('{jsonSafe}')");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error enviando datos al mapa: " + ex.Message);
            }
        }

        /// <summary>
        /// Recibe mensajes enviados desde el JS mediante 'window.chrome.webview.postMessage'.
        /// </summary>
        private void MapBrowser_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string jsonString = e.TryGetWebMessageAsString();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // Deserializamos el marcador que ha sido clicado
                var markerClicked = JsonSerializer.Deserialize<MapMarkerModel>(jsonString, options);

                if (_viewModel != null && markerClicked != null)
                {
                    // Delegamos la acción al ViewModel (ej: abrir ventana modal)
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _viewModel.MarkerClickedCommand.Execute(markerClicked);
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al procesar click desde JS: {ex.Message}");
            }
        }
    }
}