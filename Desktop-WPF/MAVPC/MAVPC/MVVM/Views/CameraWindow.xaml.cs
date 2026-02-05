using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MAVPC.Models;

namespace MAVPC
{
    public partial class CameraWindow : Window
    {
        public CameraWindow(Camara camara)
        {
            InitializeComponent();
            CargarDatos(camara);
        }

        private void CargarDatos(Camara cam)
        {
            TxtTitulo.Text = cam.Nombre;
            TxtUbicacion.Text = $"{cam.Carretera} - {cam.Direccion}";
            TxtPk.Text = $"PK {cam.Kilometro}";
            TxtCoordenadas.Text = $"X: {cam.Latitud} | Y: {cam.Longitud}";

            // Lógica del Vídeo: Solo intentamos reproducir una vez
            if (!string.IsNullOrEmpty(cam.UrlImagen))
            {
                try
                {
                    VideoPlayer.Source = new Uri(cam.UrlImagen);
                    VideoPlayer.Play();
                }
                catch
                {
                    MarcarComoOffline("ERROR URI");
                }
            }
            else
            {
                MarcarComoOffline("SIN SEÑAL");
            }
        }

        // --- EVENTOS SIMPLIFICADOS ---

        // 1. Cuando el vídeo arranca, quitamos el cartel de "Cargando"
        private void VideoPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            PnlEstado.Visibility = Visibility.Collapsed;
        }

        // 2. Si falla, mostramos error (Sin reintentos ni async)
        private void VideoPlayer_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            MarcarComoOffline("ERROR VIDEO");
        }

        // 3. Método vacío para el bucle (Opcional)
        // Lo dejo aquí pero VACÍO. Si el XAML llama a "MediaEnded", entrará aquí, 
        // no hará nada y el vídeo se parará al final sin dar error.
        private void VideoPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            // No hacemos nada. El vídeo se para.
        }

        // Método auxiliar para mostrar estado visual de error
        private void MarcarComoOffline(string mensaje)
        {
            PnlEstado.Visibility = Visibility.Visible;
            BadgeEstado.Background = new SolidColorBrush(Color.FromArgb(30, 255, 0, 0));
            BadgeEstado.BorderBrush = Brushes.Red;

            if (BadgeEstado.Child is TextBlock tb)
            {
                tb.Text = mensaje;
                tb.Foreground = Brushes.Red;
            }

            VideoPlayer.Stop();
        }

        // --- CONTROLES DE VENTANA ---

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            VideoPlayer.Stop();
            VideoPlayer.Source = null;
            this.Close();
        }
    }
}

