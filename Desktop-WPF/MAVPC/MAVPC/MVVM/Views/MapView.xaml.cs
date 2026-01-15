using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using MAVPC.Models;
using MAVPC.MVVM.ViewModels;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MAVPC.MVVM.Views
{
    public partial class MapView : UserControl
    {
        private MapViewModel? _viewModel;

        public MapView()
        {
            InitializeComponent();
            this.Unloaded += MapView_Unloaded;
        }

        private void MainMap_Loaded(object sender, RoutedEventArgs e)
        {
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            MainMap.MapProvider = GMapProviders.ArcGIS_World_Street_Map;
            MainMap.Position = new PointLatLng(42.8467, -2.6716);
            MainMap.DragButton = MouseButton.Left;
            MainMap.ShowCenter = false;

            if (DataContext is MapViewModel vm)
            {
                _viewModel = vm;
                ActualizarMapaCompleto();

                // Suscripciones
                _viewModel.Markers.CollectionChanged += OnMarkersCollectionChanged;

                // NUEVO: Escuchar cuando el usuario elige algo en el Buscador
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Si el usuario seleccionó un resultado en la lista desplegable...
            if (e.PropertyName == nameof(MapViewModel.SelectedResult))
            {
                var result = _viewModel?.SelectedResult;
                if (result != null)
                {
                    // 1. Movemos el mapa
                    MainMap.Position = new PointLatLng(result.Lat, result.Lon);
                    MainMap.Zoom = 14;

                    // 2. Abrimos la ventana correspondiente automáticamente
                    if (result.DataObject is Camara cam) new CameraWindow(cam).Show();
                    else if (result.DataObject is Incidencia inc) new IncidentWindow(inc).Show();
                }
            }
        }

        private void MapView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.Markers.CollectionChanged -= OnMarkersCollectionChanged;
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                MainMap.Dispose();
            }
        }

        // ... (OnMarkersCollectionChanged y ActualizarMapaCompleto igual que antes) ...
        private void OnMarkersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (GMapMarker m in e.NewItems) { MainMap.Markers.Add(m); ConfigurarClickMarcador(m); }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset) MainMap.Markers.Clear();
        }

        private void ActualizarMapaCompleto()
        {
            MainMap.Markers.Clear();
            if (_viewModel != null)
            {
                foreach (GMapMarker marker in _viewModel.Markers) { MainMap.Markers.Add(marker); ConfigurarClickMarcador(marker); }
            }
        }

        // Configuración de CLIC EN EL MAPA
        private void ConfigurarClickMarcador(GMapMarker marker)
        {
            if (marker.Shape is UIElement shape)
            {
                shape.MouseLeftButtonDown += (s, e) =>
                {
                    // 1. Es Cámara
                    if (marker.Tag is Camara camaraSeleccionada)
                    {
                        new CameraWindow(camaraSeleccionada).Show();
                        e.Handled = true;
                    }
                    // 2. NUEVO: Es Incidencia
                    else if (marker.Tag is Incidencia incidenciaSeleccionada)
                    {
                        new IncidentWindow(incidenciaSeleccionada).Show();
                        e.Handled = true;
                    }
                };
            }
        }
    }
}