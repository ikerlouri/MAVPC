using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ScottPlot;
using SkiaSharp;
using MAVPC.Models;
using System.Collections.Generic;
using System.Linq;
using System;

// ALIAS ESTRICTOS
using QColors = QuestPDF.Helpers.Colors;
using QFonts = QuestPDF.Helpers.Fonts;
using SColors = ScottPlot.Colors;
using SImageFormat = ScottPlot.ImageFormat;
using SColor = ScottPlot.Color;

namespace MAVPC.Services
{
    public class PdfService : IPdfService
    {
        // Paleta Corporativa
        private static readonly string HexPrimary = "#2C3E50"; // Azul Oscuro
        private static readonly string HexAccent = "#3498DB";  // Azul Brillante
        private static readonly string HexDanger = "#E74C3C";  // Rojo
        private static readonly string HexSuccess = "#27AE60"; // Verde
        private static readonly string HexLight = "#F8F9FA";   // Fondo Gris

        public void GenerateFullReport(string filePath, List<Incidencia> datos)
        {
            // --- 1. PROCESAMIENTO DE DATOS ---

            // KPIs Globales
            int total = datos.Count;
            int criticas = datos.Count(x => x.IncidenceLevel == "Rojo");
            var topProvincia = datos.GroupBy(x => x.Province ?? "N/A")
                                    .OrderByDescending(g => g.Count())
                                    .Select(g => new { Nombre = g.Key, Cantidad = g.Count() })
                                    .FirstOrDefault();

            // KPI: Top Causa (FILTRADO INTELIGENTE)
            // Ignoramos nulos, vacíos o genéricos para mostrar la causa real predominante
            var topCausa = datos
                .Where(x => !string.IsNullOrEmpty(x.Cause)
                            && x.Cause != "Var"
                            && x.Cause != "Otras"
                            && x.Cause != "Desconocida")
                .GroupBy(x => x.Cause)
                .OrderByDescending(g => g.Count())
                .Select(g => new { Nombre = g.Key, Cantidad = g.Count() })
                .FirstOrDefault();

            // Datos Gráfico Donut (Gravedad)
            var datosGravedad = datos.GroupBy(x => x.IncidenceLevel ?? "Otros")
                                   .Select(g => new { Nivel = g.Key, Cantidad = g.Count() })
                                   .OrderByDescending(x => x.Cantidad).ToList();

            // Datos Gráfico Barras (Top 5 Causas)
            var datosCausas = datos.GroupBy(x => x.Cause ?? "Otros")
                                   .Select(g => new { Causa = g.Key, Cantidad = g.Count() })
                                   .OrderByDescending(x => x.Cantidad)
                                   .Take(5)
                                   .ToList();

            // Datos Tabla (Top 20 Carreteras)
            var datosCarreteras = datos.GroupBy(x => x.Road ?? "Urbano")
                                     .Select(g => new { Carretera = g.Key, Total = g.Count() })
                                     .OrderByDescending(x => x.Total)
                                     .Take(20)
                                     .ToList();

            // Datos Tendencia (Últimos 14 días)
            var haceDias = DateTime.Now.Date.AddDays(-13);
            var tendencia = datos.Where(x => x.StartDate.HasValue && x.StartDate.Value.Date >= haceDias)
                                 .GroupBy(x => x.StartDate.Value.Date)
                                 .OrderBy(g => g.Key)
                                 .Select(g => new { Fecha = g.Key.ToString("dd/MM"), Count = (double)g.Count() })
                                 .ToList();

            // --- 2. GENERACIÓN DE GRÁFICOS ---

            byte[] imgTendencia = GenerarAreaChart(tendencia.Select(x => x.Fecha).ToArray(), tendencia.Select(x => x.Count).ToArray());
            byte[] imgGravedad = GenerarDonutChart(datosGravedad);
            byte[] imgCausas = GenerarBarChart(datosCausas);


            // --- 3. MAQUETACIÓN MULTIPÁGINA ---

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(QColors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(QFonts.SegoeUI));

                    page.Header().Element(ComposeHeader);

                    page.Content().Column(col =>
                    {
                        // === PÁGINA 1: VISIÓN GENERAL ===

                        col.Item().PaddingBottom(20).Text("Resumen Ejecutivo").FontSize(18).SemiBold().FontColor(HexPrimary);

                        // Fila de KPIs
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Element(e => KpiCard(e, "TOTAL INCIDENCIAS", total.ToString(), HexAccent));
                            row.Spacing(15);
                            row.RelativeItem().Element(e => KpiCard(e, "NIVEL ROJO", criticas.ToString(), HexDanger));
                            row.Spacing(15);
                            row.RelativeItem().Element(e => KpiCard(e, "CAUSA PRINCIPAL", topCausa?.Nombre ?? "N/A", HexSuccess));
                            row.Spacing(15);
                            row.RelativeItem().Element(e => KpiCard(e, "ZONA CRÍTICA", topProvincia?.Nombre ?? "-", HexPrimary));
                        });

                        col.Item().PaddingVertical(30).LineHorizontal(1).LineColor(QColors.Grey.Lighten3);

                        // Gráfico de Tendencia
                        col.Item().Column(c =>
                        {
                            c.Item().Text("Evolución de la Actividad (14 Días)").FontSize(14).SemiBold().FontColor(HexPrimary);
                            c.Item().Text("Tendencia diaria de incidencias registradas en la red.").FontSize(10).FontColor(QColors.Grey.Medium);
                            c.Item().PaddingTop(15).Border(1).BorderColor(QColors.Grey.Lighten4).Padding(10).Image(imgTendencia);
                        });

                        // SALTO DE PÁGINA
                        col.Item().PageBreak();


                        // === PÁGINA 2: ANÁLISIS CUALITATIVO ===

                        col.Item().PaddingBottom(20).Text("Análisis Detallado").FontSize(18).SemiBold().FontColor(HexPrimary);

                        col.Item().Row(row =>
                        {
                            // Izquierda: Gravedad (Donut)
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Distribución por Severidad").FontSize(14).SemiBold().FontColor(HexPrimary);
                                c.Item().PaddingTop(5).Text("Proporción de alertas según nivel de gravedad.").FontSize(10).FontColor(QColors.Grey.Medium);
                                c.Item().PaddingTop(20).AlignCenter().Image(imgGravedad).FitArea();
                            });

                            row.Spacing(40);

                            // Derecha: Causas (Barras)
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Principales Causas").FontSize(14).SemiBold().FontColor(HexPrimary);
                                c.Item().PaddingTop(5).Text("Top 5 motivos de incidencia reportados.").FontSize(10).FontColor(QColors.Grey.Medium);
                                c.Item().PaddingTop(20).AlignCenter().Image(imgCausas).FitArea();
                            });
                        });

                        col.Item().PaddingVertical(30).LineHorizontal(1).LineColor(QColors.Grey.Lighten3);

                        // Insights Simulados
                        col.Item().Background(HexLight).Padding(15).Column(c => {
                            c.Item().Text("INSIGHTS AUTOMÁTICOS").FontSize(10).Bold().FontColor(HexAccent);
                            double porcentajeCritico = total > 0 ? ((double)criticas / total) * 100 : 0;
                            c.Item().Text($"Se observa una concentración del {porcentajeCritico:F1}% de incidencias críticas. La provincia de {topProvincia?.Nombre} acumula la mayor carga de trabajo, sugiriendo la necesidad de reforzar recursos en dicha zona.").FontSize(10).Italic().FontColor(HexPrimary);
                        });


                        // SALTO DE PÁGINA
                        col.Item().PageBreak();


                        // === PÁGINA 3: DETALLE DE INFRAESTRUCTURA ===

                        col.Item().PaddingBottom(20).Text("Desglose por Infraestructura").FontSize(18).SemiBold().FontColor(HexPrimary);
                        col.Item().Text("Listado de los 20 puntos con mayor concentración de incidencias acumuladas.").FontSize(10).FontColor(QColors.Grey.Medium);

                        col.Item().PaddingTop(20).Element(e => ComposeTable(e, datosCarreteras));
                    });

                    // Footer Global
                    page.Footer().PaddingTop(20).Row(row => {
                        row.RelativeItem().Column(c => {
                            c.Item().Text("MAVPC Intelligence System").FontSize(10).Bold().FontColor(QColors.Grey.Darken2);
                            c.Item().Text($"Informe ID: {Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}").FontSize(8).FontColor(QColors.Grey.Medium);
                        });
                        row.RelativeItem().AlignRight().Text(x => {
                            x.Span("Página "); x.CurrentPageNumber(); x.Span(" de "); x.TotalPages();
                        });
                    });
                });
            })
            .GeneratePdf(filePath);
        }

        // --- MÉTODOS VISUALES ---

        void ComposeHeader(IContainer container)
        {
            container.PaddingBottom(20).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("INFORME ESTRATÉGICO").FontSize(28).ExtraBold().FontColor(HexPrimary);
                    c.Item().Text("Estado de la Red y Seguridad Vial").FontSize(14).FontColor(QColors.Grey.Darken1);
                });
                row.ConstantItem(150).AlignRight().Column(c =>
                {
                    c.Item().Text(DateTime.Now.ToString("dd MMMM yyyy")).FontSize(12).Bold().AlignRight();
                    c.Item().Text("MAVPC HQ").FontSize(10).FontColor(QColors.Grey.Medium).AlignRight();
                });
            });
        }

        void KpiCard(IContainer container, string label, string value, string accentColor)
        {
            container
                .BorderTop(4).BorderColor(accentColor)
                .Background(QColors.White)
                .Padding(15)
                .Column(c => {
                    c.Item().Text(value).FontSize(24).Bold().FontColor(HexPrimary);
                    // CORRECCIÓN: label.ToUpper() es C#, no QuestPDF.
                    c.Item().Text(label.ToUpper()).FontSize(8).SemiBold().FontColor(QColors.Grey.Darken1);
                });
        }

        void ComposeTable(IContainer container, dynamic datosTabla)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(40); // #
                    cols.RelativeColumn(4);  // Vía
                    cols.RelativeColumn(3);  // Provincia
                    cols.RelativeColumn(2);  // Total
                });

                table.Header(h =>
                {
                    h.Cell().Element(HeaderStyle).Text("#");
                    h.Cell().Element(HeaderStyle).Text("INFRAESTRUCTURA");
                    h.Cell().Element(HeaderStyle).Text("ESTADO");
                    h.Cell().Element(HeaderStyle).AlignRight().Text("TOTAL INC.");
                });

                int i = 1;
                foreach (var item in datosTabla)
                {
                    string bg = (i % 2 == 0) ? QColors.White : HexLight;

                    table.Cell().Element(e => CellStyle(e, bg)).Text(i.ToString()).FontColor(QColors.Grey.Medium);

                    // CORRECCIÓN: Casting explícito a (string) para que .SemiBold() funcione
                    table.Cell().Element(e => CellStyle(e, bg)).Text((string)item.Carretera).SemiBold().FontColor(HexPrimary);

                    table.Cell().Element(e => CellStyle(e, bg)).Text("Monitorizado").FontSize(9).Italic().FontColor(QColors.Grey.Darken1);

                    // CORRECCIÓN: Interpolación o .ToString() para el número
                    table.Cell().Element(e => CellStyle(e, bg)).AlignRight().Text($"{item.Total}").Bold();

                    i++;
                }
            });
        }

        static IContainer HeaderStyle(IContainer c) => c.BorderBottom(2).BorderColor(HexPrimary).Padding(8).DefaultTextStyle(x => x.SemiBold().FontSize(10).FontColor(HexPrimary));
        static IContainer CellStyle(IContainer c, string bg) => c.BorderBottom(1).BorderColor(QColors.Grey.Lighten4).Background(bg).Padding(8).DefaultTextStyle(x => x.FontSize(10));


        // --- GRÁFICOS (SCOTTPLOT 5) ---

        private byte[] GenerarAreaChart(string[] fechas, double[] valores)
        {
            if (valores.Length == 0) return new byte[0];

            ScottPlot.Plot myPlot = new();
            myPlot.FigureBackground.Color = SColors.White;
            myPlot.DataBackground.Color = SColors.White;

            myPlot.Axes.Left.FrameLineStyle.Width = 0;
            myPlot.Axes.Bottom.FrameLineStyle.Width = 0;
            myPlot.Axes.Left.TickLabelStyle.ForeColor = SColor.FromHex("#7F8C8D");
            myPlot.Axes.Bottom.TickLabelStyle.ForeColor = SColor.FromHex("#7F8C8D");

            myPlot.Grid.MajorLineColor = SColor.FromHex("#ECF0F1");
            myPlot.Axes.Bottom.MajorTickStyle.Length = 0;

            var sp = myPlot.Add.Scatter(Generate.Consecutive(valores.Length), valores);
            sp.LineWidth = 3;
            sp.Color = SColor.FromHex(HexAccent);
            sp.MarkerSize = 4;
            sp.MarkerShape = MarkerShape.FilledCircle;

            sp.FillY = true;
            sp.FillYColor = sp.Color.WithAlpha(0.2);

            ScottPlot.TickGenerators.NumericManual tickGen = new();
            for (int i = 0; i < fechas.Length; i++) tickGen.AddMajor(i, fechas[i]);
            myPlot.Axes.Bottom.TickGenerator = tickGen;
            myPlot.Axes.Bottom.TickLabelStyle.Rotation = -45;

            myPlot.Axes.Margins(bottom: 0, top: 0.2);

            return myPlot.GetImageBytes(800, 300, SImageFormat.Png);
        }

        private byte[] GenerarDonutChart(dynamic datos)
        {
            ScottPlot.Plot myPlot = new();
            myPlot.Layout.Frameless();
            myPlot.HideGrid();

            // 1. PALETA SEMÁNTICA (Prioridad)
            var cRojo = SColor.FromHex("#E74C3C");
            var cNaranja = SColor.FromHex("#E67E22");
            var cAmarillo = SColor.FromHex("#F1C40F");
            var cVerde = SColor.FromHex("#2ECC71");
            var cBlanco = SColor.FromHex("#95A5A6");

            // 2. PALETA DE RESERVA (Para todo lo que no coincida, colores bonitos y variados)
            var paletaFallback = new SColor[] {
                SColor.FromHex("#3498DB"), // Azul
                SColor.FromHex("#9B59B6"), // Morado
                SColor.FromHex("#1ABC9C"), // Turquesa
                SColor.FromHex("#34495E"), // Gris Azulado
                SColor.FromHex("#16A085"), // Verde Mar
                SColor.FromHex("#D35400"), // Calabaza
                SColor.FromHex("#8E44AD"), // Violeta
                SColor.FromHex("#2C3E50")  // Azul Noche
            };

            List<PieSlice> slices = new();
            int idx = 0; // Índice para rotar colores de fallback

            foreach (var item in datos)
            {
                string rawNivel = item.Nivel.ToString();
                string cleanLabel = rawNivel;
                SColor colorActual;
                bool semanticoEncontrado = false;

                // 3. LÓGICA DE ASIGNACIÓN
                if (rawNivel.Contains("Rojo", StringComparison.OrdinalIgnoreCase))
                {
                    cleanLabel = "Nivel Rojo"; colorActual = cRojo; semanticoEncontrado = true;
                }
                else if (rawNivel.Contains("Naran", StringComparison.OrdinalIgnoreCase))
                {
                    cleanLabel = "Nivel Naranja"; colorActual = cNaranja; semanticoEncontrado = true;
                }
                else if (rawNivel.Contains("Amar", StringComparison.OrdinalIgnoreCase))
                {
                    cleanLabel = "Nivel Amarillo"; colorActual = cAmarillo; semanticoEncontrado = true;
                }
                else if (rawNivel.Contains("Blan", StringComparison.OrdinalIgnoreCase))
                {
                    cleanLabel = "Nivel Blanco"; colorActual = cBlanco; semanticoEncontrado = true;
                }
                else if (rawNivel.Contains("Verd", StringComparison.OrdinalIgnoreCase) || rawNivel.Contains("Fluido"))
                {
                    cleanLabel = "Nivel Verde"; colorActual = cVerde; semanticoEncontrado = true;
                }
                else
                {
                    // === AQUÍ ESTÁ EL CAMBIO ===
                    // Si no reconocemos el color, usamos uno de la paleta rotatoria
                    // en lugar de usar siempre el mismo azul oscuro.
                    colorActual = paletaFallback[idx % paletaFallback.Length];

                    // Limpiamos la etiqueta si es muy larga
                    if (cleanLabel.Length > 15) cleanLabel = cleanLabel.Substring(0, 12) + "...";
                }

                slices.Add(new PieSlice { Value = item.Cantidad, FillColor = colorActual, Label = null });
                myPlot.Legend.ManualItems.Add(new LegendItem { LabelText = cleanLabel, FillColor = colorActual });

                idx++;
            }

            var pie = myPlot.Add.Pie(slices);
            pie.DonutFraction = 0.6;

            // Borde blanco (si tu versión de ScottPlot lo soporta, si no, borra estas 2 líneas)
            pie.LineStyle.Width = 2;
            pie.LineStyle.Color = SColors.White;

            myPlot.ShowLegend();
            myPlot.Legend.Orientation = Orientation.Vertical;
            myPlot.Legend.Alignment = Alignment.MiddleRight;
            myPlot.Legend.FontSize = 10;
            myPlot.Legend.FontColor = SColor.FromHex("#7F8C8D");
            myPlot.Legend.OutlineStyle.Width = 0;
            myPlot.Legend.BackgroundFill.Color = SColors.Transparent;

            myPlot.Axes.Margins(right: 0.2);

            return myPlot.GetImageBytes(500, 300, SImageFormat.Png);
        }
        private byte[] GenerarBarChart(dynamic datos)
        {
            ScottPlot.Plot myPlot = new();

            // 1. LIMPIEZA TOTAL (Quitamos Frameless para que el texto tenga 'aire')
            myPlot.HideGrid();
            myPlot.FigureBackground.Color = SColors.Transparent;

            // Quitamos las líneas de los ejes pero dejamos que se vea el texto
            myPlot.Axes.Left.FrameLineStyle.Width = 0;
            myPlot.Axes.Bottom.FrameLineStyle.Width = 0;
            myPlot.Axes.Left.TickLabelStyle.IsVisible = false; // Los números ya van arriba de las barras

            List<Bar> bars = new();
            List<Tick> ticks = new();
            int i = 0;

            foreach (var item in datos)
            {
                var bar = new Bar()
                {
                    Position = i,
                    Value = item.Cantidad,
                    FillColor = SColor.FromHex(HexPrimary)
                };
                // El número que ves arriba de la barra
                bar.Label = item.Cantidad.ToString();
                bars.Add(bar);

                // --- EL CAMBIO: NO CORTAMOS EL NOMBRE ---
                string label = item.Causa.ToString();
                ticks.Add(new Tick(i, label));
                i++;
            }

            var barPlot = myPlot.Add.Bars(bars);
            barPlot.ValueLabelStyle.FontSize = 13;
            barPlot.ValueLabelStyle.Bold = true;

            // --- CONFIGURACIÓN DEL EJE X (LOS NOMBRES) ---
            myPlot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks.ToArray());
            myPlot.Axes.Bottom.TickLabelStyle.IsVisible = true;
            myPlot.Axes.Bottom.TickLabelStyle.FontSize = 11;
            myPlot.Axes.Bottom.TickLabelStyle.ForeColor = SColor.FromHex("#2C3E50");

            // Rotamos a -45 grados: Es la única forma de que nombres como "Mantenimiento" 
            // no se pisen con los de al lado.
            myPlot.Axes.Bottom.TickLabelStyle.Rotation = -45;
            myPlot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleRight;

            // --- EL MARGEN FUNDAMENTAL ---
            // Al dar un 0.4 abajo, reservamos casi la mitad de la imagen para que quepan los nombres
            myPlot.Axes.Margins(bottom: 0.45, top: 0.2);

            return myPlot.GetImageBytes(500, 350, SImageFormat.Png);
        }
    }
}


