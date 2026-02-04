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
    public partial class MapView : UserControl
    {
        private MapViewModel? _viewModel;
        private bool _isMapLoaded = false;

        public MapView()
        {
            InitializeComponent();
            InitializeAsync();
            this.DataContextChanged += MapView_DataContextChanged;
        }

        private async void InitializeAsync()
        {
            try
            {
                await MapBrowser.EnsureCoreWebView2Async();

                string currentDir = AppDomain.CurrentDomain.BaseDirectory;
                string mapPath = Path.Combine(currentDir, "Assets", "mapa.html");

                if (File.Exists(mapPath))
                {
                    MapBrowser.WebMessageReceived += MapBrowser_WebMessageReceived;
                    // Truco: Convertimos a URI para evitar problemas de rutas raras en Windows
                    MapBrowser.CoreWebView2.Navigate(new Uri(mapPath).AbsoluteUri);
                    MapBrowser.NavigationCompleted += MapBrowser_NavigationCompleted;
                }
                else
                {
                    MessageBox.Show($"Falta el archivo: {mapPath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error iniciando WebView2: " + ex.Message);
            }
        }

        // Se ejecuta cuando el HTML ha terminado de cargar
        private void MapBrowser_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            _isMapLoaded = true;
            RefrescarMapa(); // Si ya había datos, los pintamos ahora
        }

        private void MapView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is MapViewModel vm)
            {
                _viewModel = vm;
                // Nos suscribimos a cambios en la lista de marcadores
                _viewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == "Markers")
                    {
                        RefrescarMapa();
                    }
                };
                // Intento inicial por si ya hay datos
                RefrescarMapa();
            }
        }

        private async void RefrescarMapa()
        {
            // Verificamos que todo esté listo para no dar errores tontos
            if (!_isMapLoaded || _viewModel == null || _viewModel.Markers == null || MapBrowser.CoreWebView2 == null) return;

            try
            {
                // 1. Serializamos a JSON
                var options = new JsonSerializerOptions { PropertyNamingPolicy = null };
                var json = JsonSerializer.Serialize(_viewModel.Markers, options);

                // --- CORRECCIÓN CRÍTICA ---
                // Escapamos las comillas simples (') porque si no rompen la cadena de JS
                // Ejemplo: "Sant'Ana" rompía la llamada CargarMarcadores('...')
                var jsonSafe = json.Replace("'", "\\'");

                // 2. Ejecutamos la función JS
                await MapBrowser.CoreWebView2.ExecuteScriptAsync($"CargarMarcadores('{jsonSafe}')");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error enviando datos al mapa: " + ex.Message);
            }
        }

        // Recibe el click desde el JS
        private void MapBrowser_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                string jsonString = e.TryGetWebMessageAsString();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var markerClicked = JsonSerializer.Deserialize<MapMarkerModel>(jsonString, options);

                if (_viewModel != null && markerClicked != null)
                {
                    // Ejecutamos el comando en el hilo de UI
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _viewModel.MarkerClickedCommand.Execute(markerClicked);
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al procesar click: {ex.Message}");
            }
        }
    }
}