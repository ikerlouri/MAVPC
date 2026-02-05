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
        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                // Soporte para maximizar con doble clic
                if (e.ClickCount == 2)
                {
                    if (WindowState == WindowState.Maximized)
                        WindowState = WindowState.Normal;
                    else
                        WindowState = WindowState.Maximized;
                }
                else
                {
                    DragMove();
                }
            }
        }
    }
}