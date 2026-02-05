using CommunityToolkit.Mvvm.ComponentModel;
using LiveCharts;
using LiveCharts.Wpf;
using MAVPC.Models;
using MAVPC.Services;
using System;
using System.Linq;
using System.Windows.Media;

namespace MAVPC.MVVM.ViewModels
{
    public partial class StatsViewModel : ObservableObject
    {
        private readonly ITrafficService _trafficService;

        // --- KPIS ---
        [ObservableProperty] private string _kpiTotal = "0";
        [ObservableProperty] private string _kpiCriticas = "0";
        [ObservableProperty] private string _kpiTopCausa = "-";
        [ObservableProperty] private string _kpiTopZona = "-";

        // --- PROPIEDADES GRÁFICOS ---
        [ObservableProperty] private SeriesCollection _incidenciasSeries;
        [ObservableProperty] private SeriesCollection _camarasSeries;
        [ObservableProperty] private SeriesCollection _carreterasSeries;

        [ObservableProperty] private string[] _labels;
        [ObservableProperty] private string[] _carreterasLabels;
        [ObservableProperty] private Func<double, string> _formatter;

        // --- NUEVO: CONSTRUCTOR VACÍO DE SEGURIDAD ---
        // Esto evita que el XAML explote si intenta crear el VM sin pasarle el servicio
        public StatsViewModel()
        {
            // Inicializamos listas para que no de error de "Null Reference" al pintar
            IncidenciasSeries = new SeriesCollection();
            CamarasSeries = new SeriesCollection();
            CarreterasSeries = new SeriesCollection();
            Formatter = value => value.ToString("N0");
        }

        // --- CONSTRUCTOR PRINCIPAL ---
        public StatsViewModel(ITrafficService trafficService) : this() // Llama al vacío primero para inicializar listas
        {
            _trafficService = trafficService;
            LoadStats();
        }

        private async void LoadStats()
        {
            // NUEVO: Comprobación de seguridad
            if (_trafficService == null) return;

            try
            {
                var incidencias = await _trafficService.GetIncidenciasAsync();
                var camaras = await _trafficService.GetCamarasAsync();

                if (incidencias == null || camaras == null) return;

                // 1. CÁLCULO DE KPIS
                KpiTotal = incidencias.Count.ToString();
                KpiCriticas = incidencias.Count(x => x.IncidenceLevel == "Rojo").ToString();

                var topCausaGroup = incidencias
                    .Where(x => !string.IsNullOrEmpty(x.Cause) && x.Cause != "Var")
                    .GroupBy(x => x.Cause)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();
                KpiTopCausa = topCausaGroup?.Key ?? "N/A";

                var topZonaGroup = incidencias
                    .Where(x => !string.IsNullOrEmpty(x.Province))
                    .GroupBy(x => x.Province)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();
                KpiTopZona = topZonaGroup?.Key ?? "N/A";

                // 2. DONUT TIPOS
                var pieSeries = new SeriesCollection();
                var rawTipos = incidencias
                    .GroupBy(x => x.IncidenceType ?? "OTROS")
                    .Select(g => new { Tipo = g.Key, Cantidad = g.Count() })
                    .OrderByDescending(x => x.Cantidad);

                var colores = new[] { "#00FFFF", "#FF007F", "#FFA500", "#FFFFFF", "#32CD32" };
                int i = 0;

                foreach (var item in rawTipos)
                {
                    var colorHex = colores[i % colores.Length];
                    pieSeries.Add(new PieSeries
                    {
                        Title = item.Tipo,
                        Values = new ChartValues<int> { item.Cantidad },
                        DataLabels = true,
                        LabelPoint = point => "",
                        Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(colorHex),
                        StrokeThickness = 2,
                        Stroke = Brushes.Black
                    });
                    i++;
                }
                IncidenciasSeries = pieSeries;

                // 3. BARRAS CARRETERAS
                var topRoads = incidencias
                    .Where(x => !string.IsNullOrEmpty(x.Road))
                    .GroupBy(x => x.Road)
                    .OrderByDescending(g => g.Count())
                    .Take(7)
                    .Select(g => new { Carretera = g.Key, Cantidad = g.Count() })
                    .ToList();

                CarreterasSeries = new SeriesCollection
                {
                    new RowSeries
                    {
                        Title = "Incidencias",
                        Values = new ChartValues<int>(topRoads.Select(x => x.Cantidad)),
                        Fill = (SolidColorBrush)new BrushConverter().ConvertFromString("#00FFFF"),
                        DataLabels = true,
                        LabelPoint = p => p.X.ToString()
                    }
                };
                CarreterasLabels = topRoads.Select(x => x.Carretera).ToArray();

                // 4. COLUMNAS ACTIVOS
                CamarasSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Activos",
                        Values = new ChartValues<int> { camaras.Count, incidencias.Count },
                        Fill = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF007F"),
                        DataLabels = true,
                        MaxColumnWidth = 50
                    }
                };
                Labels = new[] { "Cámaras", "Incidencias" };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stats: {ex.Message}");
            }
        }
    }
}