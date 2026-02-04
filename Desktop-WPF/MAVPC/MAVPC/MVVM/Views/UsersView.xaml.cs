using System.Windows.Controls;
using System.Windows.Input;
using MAVPC.MVVM.ViewModels; // Asegúrate de tener este using

namespace MAVPC.MVVM.Views
{
    public partial class UsersView : UserControl
    {
        public UsersView()
        {
            InitializeComponent();
        }

        // Si hacen click en el fondo oscuro (Overlay)
        private void Overlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is UsersViewModel vm)
            {
                vm.CloseDialogCommand.Execute(null);
            }
        }

        // Si hacen click dentro de la tarjeta, DETENEMOS el evento para que no llegue al fondo
        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}