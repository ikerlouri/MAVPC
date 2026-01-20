using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MAVPC.MVVM.Converters
{
    // 1. CONVERTER DE COLOR (Corregido para detectar Montaña)
    public class IncidentTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Pasamos a minúsculas para comparar fácil
            string type = value?.ToString()?.ToLower() ?? "";

            // CASO 1: ACCIDENTES (Rojo)
            if (type.Contains("accidente") || type.Contains("rojo") || type.Contains("grave") || type.Contains("cerrado"))
                return Brushes.Red;

            // CASO 2: OBRAS (Naranja)
            if (type.Contains("obra") || type.Contains("mantenimiento"))
                return Brushes.Orange;

            // CASO 3: TRÁFICO (Amarillo)
            if (type.Contains("retención") || type.Contains("tráfico") || type.Contains("lento"))
                return Brushes.Yellow;

            // CASO 4: NIEVE / PUERTOS (Blanco / Nieve) -> ¡ESTO ES LO QUE FALTABA!
            if (type.Contains("puerto") || type.Contains("montaña") || type.Contains("nieve") || type.Contains("hielo") || type.Contains("climatología"))
                return Brushes.White;

            // CASO 5: CÁMARAS (Si las usas aquí)
            if (type.Contains("cámara") || type.Contains("camara"))
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00D4FF")); // Cyan Tecnológico

            // DEFAULT: Si no sabemos qué es, ponemos el Cyan (o podrías poner Gris para que no destaque tanto)
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00D4FF"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // 2. CONVERTER DE ICONO (Corregido para detectar Montaña)
    public class IncidentTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string type = value?.ToString()?.ToLower() ?? "";

            if (type.Contains("accidente")) return PackIconKind.Car;
            if (type.Contains("obra") || type.Contains("mantenimiento")) return PackIconKind.Cone; // Cone (cono) es muy visual

            // Aquí añadimos "puerto" y "montaña" para que salga el icono de clima
            if (type.Contains("lluvia") || type.Contains("nieve") || type.Contains("hielo") || type.Contains("puerto") || type.Contains("montaña"))
                return PackIconKind.Snowflake; // O PackIconKind.Mountain si prefieres

            if (type.Contains("retención")) return PackIconKind.TrafficLight;

            return PackIconKind.AlertCircleOutline;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}