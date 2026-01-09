using System.Windows;
using System.Windows.Input;

namespace MAVPC.Views
{
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
        }

        // Permite mover la ventana al arrastrar la barra superior
        private void TopBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}