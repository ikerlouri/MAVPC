using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace MAVPC.MVVM.Converters
{
    // Convierte el texto de la incidencia en un Color (Neon)
    public class IncidentTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string type = value?.ToString()?.ToLower() ?? "";

            if (type.Contains("accidente") || type.Contains("rojo") || type.Contains("grave"))
                return Brushes.Red; // O #FF0055 para Neon Pink
            if (type.Contains("obra") || type.Contains("mantenimiento"))
                return Brushes.Orange;
            if (type.Contains("retención") || type.Contains("tráfico"))
                return Brushes.Yellow;

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00FFFF")); // Cyan por defecto
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // Convierte el texto de la incidencia en un Icono de Material Design
    public class IncidentTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string type = value?.ToString()?.ToLower() ?? "";

            if (type.Contains("accidente")) return PackIconKind.Car;
            if (type.Contains("obra")) return PackIconKind.Construction;
            if (type.Contains("lluvia") || type.Contains("nieve")) return PackIconKind.WeatherPouring;

            return PackIconKind.AlertCircleOutline;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}