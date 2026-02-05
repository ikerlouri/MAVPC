using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace MAVPC.MVVM.Converters
{
    // 1. INVERSE BOOL (Este se queda igual porque no está en el otro archivo)
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue) return !booleanValue;
            return false;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // 2. ICON CONVERTER DEL MAPA (Renombrado a MapIncident...)
    public class MapIncidentTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string type = (value as string ?? "").ToLower();

            if (type.Contains("camara")) return PackIconKind.Cctv;
            if (type.Contains("obra") || type.Contains("mantenimiento")) return PackIconKind.Construction;
            if (type.Contains("accidente") || type.Contains("grave")) return PackIconKind.Car;
            if (type.Contains("lluvia") || type.Contains("nieve") || type.Contains("hielo")) return PackIconKind.WeatherRainy;
            if (type.Contains("retención") || type.Contains("tráfico")) return PackIconKind.TrafficLight;

            return PackIconKind.AlertCircle;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // 3. COLOR CONVERTER DEL MAPA (Renombrado a MapIncident...)
    public class MapIncidentTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string type = (value as string ?? "").ToLower();

            if (type.Contains("camara")) return Brushes.DodgerBlue;
            if (type.Contains("obra") || type.Contains("mantenimiento")) return Brushes.Orange;
            if (type.Contains("retención") || type.Contains("tráfico")) return Brushes.Yellow;

            return Brushes.Red;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}

