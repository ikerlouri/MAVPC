using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace MAVPC.MVVM.Converters
{
    /// <summary>
    /// Invierte un valor booleano. Útil para propiedades de Visibilidad o IsEnabled.
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue) return !booleanValue;
            return false;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue) return !booleanValue;
            return false;
        }
    }

    /// <summary>
    /// Lógica de Iconos específica para la vista del Mapa (SidePanel).
    /// </summary>
    public class MapIncidentTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string type || string.IsNullOrEmpty(type))
                return PackIconKind.AlertCircle;

            if (Contains(type, "camara")) return PackIconKind.Cctv;
            if (Contains(type, "obra") || Contains(type, "mantenimiento")) return PackIconKind.Construction;
            if (Contains(type, "accidente") || Contains(type, "grave")) return PackIconKind.Car;
            if (Contains(type, "lluvia") || Contains(type, "nieve") || Contains(type, "hielo")) return PackIconKind.WeatherRainy;
            if (Contains(type, "retención") || Contains(type, "tráfico")) return PackIconKind.TrafficLight;

            return PackIconKind.AlertCircle;
        }

        private bool Contains(string source, string keyword)
            => source.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }

    /// <summary>
    /// Lógica de Colores simplificada para marcadores o bordes en el mapa.
    /// </summary>
    public class MapIncidentTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not string type || string.IsNullOrEmpty(type))
                return Brushes.Red; // Default alert

            if (Contains(type, "camara")) return Brushes.DodgerBlue;
            if (Contains(type, "obra") || Contains(type, "mantenimiento")) return Brushes.Orange;
            if (Contains(type, "retención") || Contains(type, "tráfico")) return Brushes.Yellow;

            return Brushes.Red;
        }

        private bool Contains(string source, string keyword)
            => source.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
    }
}