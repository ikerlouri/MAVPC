using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MAVPC.Models; // Necesario para MapMarkerModel
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
            await MapBrowser.EnsureCoreWebView2Async();

            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            string mapPath = Path.Combine(currentDir, "Assets", "mapa.html");

            if (File.Exists(mapPath))
            {
                // AÑADIDO: Escuchar mensajes desde JS (los clicks en iconos)
                MapBrowser.WebMessageReceived += MapBrowser_WebMessageReceived;

                MapBrowser.CoreWebView2.Navigate(mapPath);
                MapBrowser.NavigationCompleted += MapBrowser_NavigationCompleted;
            }
            else
            {
                MessageBox.Show($"No se encuentra el mapa en: {mapPath}", "Error de Archivo");
            }
        }

        // --- NUEVO: Manejador del mensaje que viene de JS ---
        private void MapBrowser_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                // 1. Obtenemos el JSON crudo del marcador pulsado
                string jsonString = e.TryGetWebMessageAsString();

                // 2. Configuramos opciones para evitar problemas de mayúsculas/minúsculas
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // 3. Convertimos a nuestro modelo genérico
                var markerClicked = JsonSerializer.Deserialize<MapMarkerModel>(jsonString, options);

                // 4. Si todo va bien, ejecutamos el comando del ViewModel
                if (_viewModel != null && markerClicked != null)
                {
                    _viewModel.MarkerClickedCommand.Execute(markerClicked);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error recibiendo click del mapa: {ex.Message}");
            }
        }

        private void MapBrowser_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            _isMapLoaded = true;
            RefrescarMapa();
        }

        private void MapView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is MapViewModel vm)
            {
                _viewModel = vm;
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

        private async void RefrescarMapa()
        {
            if (!_isMapLoaded || _viewModel == null || _viewModel.Markers == null) return;

            try
            {
                // Serializamos ignorando nomenclatura para asegurar que JS lo lea bien
                var options = new JsonSerializerOptions { PropertyNamingPolicy = null };
                var json = JsonSerializer.Serialize(_viewModel.Markers, options);

                await MapBrowser.CoreWebView2.ExecuteScriptAsync($"CargarMarcadores('{json}')");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al pintar mapa: " + ex.Message);
            }
        }
    }
}