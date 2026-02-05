using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MAVPC.MVVM.Converters
{
    /// <summary>
    /// Convierte tipos de incidencia (string) a Colores (Brush) para el Dashboard.
    /// Optimizado para evitar asignaciones de memoria en listas largas.
    /// </summary>
    public class IncidentTypeToColorConverter : IValueConverter
    {
        // Cacheamos el color personalizado para no crearlo en cada fila (Performance crítica en listas)
        private static readonly SolidColorBrush CyanBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00D4FF"));

        static IncidentTypeToColorConverter()
        {
            // Congelar el brush permite que WPF lo comparta entre hilos y objetos sin overhead
            if (CyanBrush.CanFreeze) CyanBrush.Freeze();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string type || string.IsNullOrEmpty(type))
                return CyanBrush; // Default

            // Uso de StringComparison para evitar .ToLower() (Más rápido)
            if (IsMatch(type, "accidente", "rojo", "grave", "cerrado")) return Brushes.Red;
            if (IsMatch(type, "obra", "mantenimiento")) return Brushes.Orange;
            if (IsMatch(type, "retención", "tráfico", "lento")) return Brushes.Yellow;

            // Lógica específica para Meteo/Montaña
            if (IsMatch(type, "puerto", "montaña", "nieve", "hielo", "climatología")) return Brushes.White;

            return CyanBrush;
        }

        // Helper para comparar múltiples keywords eficientemente
        private bool IsMatch(string source, params string[] keywords)
        {
            foreach (var key in keywords)
            {
                if (source.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    /// <summary>
    /// Convierte tipos de incidencia a Iconos de Material Design.
    /// </summary>
    public class IncidentTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string type || string.IsNullOrEmpty(type))
                return PackIconKind.AlertCircleOutline;

            if (Contains(type, "accidente")) return PackIconKind.Car; 
            if (Contains(type, "obra") || Contains(type, "mantenimiento")) return PackIconKind.Cone;

            // Meteo / Montaña
            if (Contains(type, "lluvia") || Contains(type, "nieve") || Contains(type, "hielo") || Contains(type, "puerto") || Contains(type, "montaña"))
                return PackIconKind.Snowflake;

            if (Contains(type, "retención")) return PackIconKind.TrafficLight;

            return PackIconKind.AlertCircleOutline;
        }

        private bool Contains(string source, string keyword)
            => source.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}