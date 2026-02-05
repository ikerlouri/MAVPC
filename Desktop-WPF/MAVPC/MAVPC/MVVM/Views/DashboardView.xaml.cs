using MAVPC.Models;
using System.Windows;
using System.Windows.Controls;

namespace MAVPC.MVVM.Views
{
    public partial class DashboardView : UserControl
    {
        public DashboardView()
        {
            InitializeComponent();
        }

        private void BtnVerCamara_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            var camara = boton?.DataContext as Camara;

            if (camara != null)
            {
                // Creamos y mostramos la ventana estilo "popup"
                var ventana = new CameraWindow(camara);
                ventana.ShowDialog(); // ShowDialog bloquea el fondo hasta que cierras
            }
        }

        private void MediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            // Este código hace el bucle: cuando el vídeo acaba, vuelve al inicio (0) y reproduce
            if (sender is MediaElement media)
            {
                media.Position = TimeSpan.Zero;
                media.Play();
            }
        }    
    }
}
============================================================
ARCHIVO: C:\Users\2dam3\Documents\Retos\MAVPC\Desktop-WPF\MAVPC\MAVPC\MVVM\Views\DashboardView.xaml.cs
============================================================
