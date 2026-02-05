using System;
using System.Windows;
using System.Windows.Input;

namespace MAVPC.MVVM.Views
{
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
        }

        // Permite mover la ventana al arrastrar la barra superior
        // Y maximizar si se hace doble clic
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                if (e.ClickCount == 2)
                {
                    // Doble clic -> Maximizar/Restaurar
                    MaximizeButton_Click(sender, e);
                }
                else
                {
                    // Clic simple y arrastrar -> Mover ventana
                    DragMove();
                }
            }
        }

        // Minimizar a la barra de tareas
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Alternar entre Maximizado y Normal
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
            {
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.WindowState = WindowState.Normal;
            }
        }

        // Cerrar la aplicación por completo
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Desvinculamos el DataContext para limpieza (opcional pero recomendado)
            this.DataContext = null;
            Environment.Exit(0);
        }
    }
}