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
    /// <summary>
    /// ViewModel encargado de la analítica de datos.
    /// Calcula KPIs en tiempo real y genera series de datos para gráficas (LiveCharts).
    /// </summary>
    public partial class StatsViewModel : ObservableObject
    {
        private readonly ITrafficService _trafficService;

        #region Propiedades KPI (Indicadores Clave)

        /// <summary>Total de incidencias registradas.</summary>
        [ObservableProperty] private string _kpiTotal = "0";

        /// <summary>Total de incidencias de nivel Rojo/Crítico.</summary>
        [ObservableProperty] private string _kpiCriticas = "0";

        /// <summary>La causa más frecuente de incidencias.</summary>
        [ObservableProperty] private string _kpiTopCausa = "-";

        /// <summary>La provincia o zona con más incidencias.</summary>
        [ObservableProperty] private string _kpiTopZona = "-";

        #endregion

        #region Propiedades Gráficos (LiveCharts)

        /// <summary>Datos para el gráfico circular de Tipos.</summary>
        [ObservableProperty] private SeriesCollection _incidenciasSeries;

        /// <summary>Datos para el gráfico de barras comparativo (Cámaras vs Incidencias).</summary>
        [ObservableProperty] private SeriesCollection _camarasSeries;

        /// <summary>Datos para el gráfico de barras de Carreteras conflictivas.</summary>
        [ObservableProperty] private SeriesCollection _carreterasSeries;

        // Ejes y Formateadores
        [ObservableProperty] private string[] _labels;
        [ObservableProperty] private string[] _carreterasLabels;
        [ObservableProperty] private Func<double, string> _formatter;

        #endregion

        #region Constructores

        /// <summary>
        /// Constructor vacío de seguridad.
        /// Necesario para que el diseñador XAML de Visual Studio no lance excepciones al intentar renderizar la vista.
        /// </summary>
        public StatsViewModel()
        {
            // Inicialización preventiva de colecciones para evitar NullReferenceException
            IncidenciasSeries = new SeriesCollection();
            CamarasSeries = new SeriesCollection();
            CarreterasSeries = new SeriesCollection();
            Formatter = value => value.ToString("N0");
        }

        /// <summary>
        /// Constructor principal con inyección de dependencias.
        /// </summary>
        public StatsViewModel(ITrafficService trafficService) : this() // Llama al vacío primero
        {
            _trafficService = trafficService;
            LoadStats();
        }

        #endregion

        /// <summary>
        /// Carga los datos de los servicios, calcula los KPIs y rellena las series de los gráficos.
        /// </summary>
        private async void LoadStats()
        {
            // Comprobación de seguridad por si el servicio falla en la inyección
            if (_trafficService == null) return;

            try
            {
                var incidencias = await _trafficService.GetIncidenciasAsync();
                var camaras = await _trafficService.GetCamarasAsync();

                if (incidencias == null || camaras == null) return;

                // --- 1. CÁLCULO DE KPIS ---
                KpiTotal = incidencias.Count().ToString();

                // Filtramos por nivel Rojo (Crítico)
                KpiCriticas = incidencias.Count(x => x.IncidenceLevel == "Rojo").ToString();

                // Calculamos la causa más común ignorando valores vacíos o "Var"
                var topCausaGroup = incidencias
                    .Where(x => !string.IsNullOrEmpty(x.Cause) && x.Cause != "Var")
                    .GroupBy(x => x.Cause)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();
                KpiTopCausa = topCausaGroup?.Key ?? "N/A";

                // Calculamos la zona (Provincia) con más actividad
                var topZonaGroup = incidencias
                    .Where(x => !string.IsNullOrEmpty(x.Province))
                    .GroupBy(x => x.Province)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault();
                KpiTopZona = topZonaGroup?.Key ?? "N/A";


                // --- 2. GRÁFICO DONUT (Distribución por Tipos) ---
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
                        LabelPoint = point => "", // Ocultamos etiqueta interna para limpieza
                        Fill = (SolidColorBrush)new BrushConverter().ConvertFromString(colorHex),
                        StrokeThickness = 2,
                        Stroke = Brushes.Black
                    });
                    i++;
                }
                IncidenciasSeries = pieSeries;


                // --- 3. GRÁFICO BARRAS HORIZONTALES (Top Carreteras) ---
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


                // --- 4. GRÁFICO COLUMNAS (Activos: Cámaras vs Incidencias) ---
                CamarasSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Activos",
                        Values = new ChartValues<int> { camaras.Count(), incidencias.Count() },
                        Fill = (SolidColorBrush)new BrushConverter().ConvertFromString("#FF007F"),
                        DataLabels = true,
                        MaxColumnWidth = 50
                    }
                };
                Labels = new[] { "Cámaras", "Incidencias" };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error calculando estadísticas: {ex.Message}");
            }
        }
    }
}