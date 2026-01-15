using MAVPC.Models;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MAVPC
{
    public partial class IncidentWindow : Window
    {
        public IncidentWindow(Incidencia inc)
        {
            InitializeComponent();
            CargarDatos(inc);
        }

        private void CargarDatos(Incidencia inc)
        {
            // 1. Tipo y Causa
            // Usamos el operador ?. y ?? para evitar crasheos si viene null
            TxtTipo.Text = (inc.IncidenceType ?? "INCIDENCIA").ToUpper();
            TxtCausa.Text = string.IsNullOrEmpty(inc.Cause) ? (inc.IncidenceType ?? "-") : inc.Cause;

            // 2. Ubicación
            // OJO: La API nueva NO trae Kilómetro (PK), así que solo mostramos la carretera
            TxtMunicipio.Text = $"{inc.CityTown ?? "Ubicación desconocida"} ({inc.Province ?? "-"})";
            TxtCarretera.Text = inc.Road ?? "Vía desconocida";

            // 3. Dirección y Fecha
            TxtDireccion.Text = !string.IsNullOrEmpty(inc.Direction) ? $" ➜ {inc.Direction}" : "";
            TxtFecha.Text = inc.StartDate.HasValue
                ? inc.StartDate.Value.ToString("dd/MM/yyyy HH:mm")
                : "--/--/----";

            // 4. Lógica Visual (Colores e Iconos)
            try
            {
                var colorHex = inc.StatusColor;
                var brush = (SolidColorBrush)new BrushConverter().ConvertFrom(colorHex)!;

                // Aplicar color al borde principal y al icono
                MainBorder.BorderBrush = brush;
                IconoIncidencia.Foreground = brush;

                // Convertir string a Enum de Icono
                if (Enum.TryParse(typeof(MaterialDesignThemes.Wpf.PackIconKind), inc.IconKind, out var iconKind))
                {
                    IconoIncidencia.Kind = (MaterialDesignThemes.Wpf.PackIconKind)iconKind;
                }

                // Configurar el Badge de Nivel
                BadgeNivel.BorderBrush = brush;
                BadgeNivel.Background = new SolidColorBrush(Color.FromArgb(30, brush.Color.R, brush.Color.G, brush.Color.B));

                TxtNivel.Text = (inc.IncidenceLevel ?? "INFO").ToUpper();
                TxtNivel.Foreground = brush;
            }
            catch
            {
                // Fallback de seguridad por si falla la conversión de color/icono
                MainBorder.BorderBrush = Brushes.Gray;
            }
        }

        // --- Eventos de Ventana ---

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}