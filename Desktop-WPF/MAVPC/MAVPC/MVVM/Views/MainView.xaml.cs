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

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Opcional: Desvincula el DataContext para que LiveCharts deje de escuchar
            this.DataContext = null;

            // Mata el proceso de golpe. Al ser un evento directo, no da error de Reflexión.
            Environment.Exit(0);
        }
    }
}