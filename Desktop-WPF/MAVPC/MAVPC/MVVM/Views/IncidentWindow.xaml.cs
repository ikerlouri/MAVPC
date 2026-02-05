using MAVPC.Models;
using System.Windows;
using System.Windows.Input;

namespace MAVPC
{
    public partial class IncidentWindow : Window
    {
        private Incidencia _incidencia;

        public IncidentWindow(Incidencia inc)
        {
            InitializeComponent();
            _incidencia = inc;
            this.DataContext = _incidencia;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void BtnCopiar_Click(object sender, RoutedEventArgs e)
        {
            if (_incidencia.Latitude.HasValue && _incidencia.Longitude.HasValue)
            {
                string data = $"{_incidencia.Latitude.Value}, {_incidencia.Longitude.Value}";
                Clipboard.SetText(data);
                // Usamos un Snackbar o MessageBox discreto
                MessageBox.Show($"Copiado al portapapeles:\n{data}", "Coordenadas", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
============================================================
ARCHIVO: C:\Users\2dam3\Documents\Retos\MAVPC\Desktop-WPF\MAVPC\MAVPC\MVVM\Views\IncidentWindow.xaml.cs
============================================================
