using CommunityToolkit.Mvvm.ComponentModel;
using LiveCharts;
using LiveCharts.Wpf;
using MAVPC.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MAVPC.MVVM.ViewModels
{
    public partial class StatsViewModel : ObservableObject
    {
        private readonly ITrafficService _trafficService;

        // Propiedades de Datos
        [ObservableProperty] private SeriesCollection _incidenciasSeries;
        [ObservableProperty] private SeriesCollection _camarasSeries;
        [ObservableProperty] private SeriesCollection _carreterasSeries;

        [ObservableProperty] private string[] _labels;
        [ObservableProperty] private string[] _carreterasLabels;

        [ObservableProperty] private Func<double, string> _formatter;

        public StatsViewModel(ITrafficService trafficService)
        {
            _trafficService = trafficService;

            // Inicializar para evitar nulos
            IncidenciasSeries = new SeriesCollection();
            CamarasSeries = new SeriesCollection();
            CarreterasSeries = new SeriesCollection();

            // Formateador: Convierte 10.0 en "10"
            Formatter = value => value.ToString("N0");

            // Cargamos los datos (async void es aceptable en constructor/eventos de UI)
            LoadStats();
        }

        private async void LoadStats()
        {
            try
            {
                var incidencias = await _trafficService.GetIncidenciasAsync();
                var camaras = await _trafficService.GetCamarasAsync();

                if (incidencias == null || camaras == null) return;

                // ==========================================
                // 1. GRÁFICO CIRCULAR (Por Tipo de Incidencia)
                // ==========================================
                // CAMBIO: Usamos 'IncidenceType' en lugar de 'Tipo'
                var incidenciasPorTipo = incidencias
                    .GroupBy(x => x.IncidenceType ?? "OTROS") // Agrupamos, si es null ponemos OTROS
                    .Select(g => new { Tipo = g.Key, Cantidad = g.Count() });

                var pieSeries = new SeriesCollection();
                foreach (var item in incidenciasPorTipo)
                {
                    pieSeries.Add(new PieSeries
                    {
                        Title = item.Tipo,
                        Values = new ChartValues<int> { item.Cantidad },
                        DataLabels = true,
                        LabelPoint = chartPoint => string.Format("{0} ({1:P})", chartPoint.SeriesView.Title, chartPoint.Participation)
                    });
                }
                IncidenciasSeries = pieSeries;

                // ==========================================
                // 2. GRÁFICO COLUMNAS (Totales)
                // ==========================================
                CamarasSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Total",
                        Values = new ChartValues<int> { camaras.Count, incidencias.Count },
                        Fill = System.Windows.Media.Brushes.OrangeRed
                    }
                };
                Labels = new[] { "Cámaras", "Incidencias" };

                // ==========================================
                // 3. GRÁFICO BARRAS HORIZONTALES (Top 5 Carreteras)
                // ==========================================
                // CAMBIO: Usamos 'Road' en lugar de 'Carretera'
                var topRoads = incidencias
                    .Where(x => !string.IsNullOrEmpty(x.Road)) // Filtramos nulos
                    .GroupBy(x => x.Road)
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .Select(g => new { Carretera = g.Key, Cantidad = g.Count() })
                    .ToList();

                CarreterasSeries = new SeriesCollection
                {
                    new RowSeries
                    {
                        Title = "Incidencias",
                        Values = new ChartValues<int>(topRoads.Select(x => x.Cantidad)),
                        Fill = System.Windows.Media.Brushes.DodgerBlue
                    }
                };

                // Asignamos las etiquetas (eje Y) con los nombres de las carreteras
                CarreterasLabels = topRoads.Select(x => x.Carretera).ToArray();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando estadísticas: {ex.Message}");
            }
        }
    }
}
============================================================
ARCHIVO: C:\Users\2dam3\Documents\Retos\MAVPC\Desktop-WPF\MAVPC\MAVPC\MVVM\ViewModels\StatsViewModel.cs
============================================================
