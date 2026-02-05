using System.Windows.Controls;
using System.Windows.Input;
using MAVPC.MVVM.ViewModels;

namespace MAVPC.MVVM.Views
{
    /// <summary>
    /// Lógica de interacción para UsersView.xaml.
    /// Gestiona eventos de ratón para el comportamiento del diálogo modal.
    /// </summary>
    public partial class UsersView : UserControl
    {
        public UsersView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Detecta el clic en el fondo oscuro (Overlay) para cerrar el diálogo.
        /// </summary>
        private void Overlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is UsersViewModel vm)
            {
                // Ejecutamos el comando de cerrar diálogo del ViewModel
                if (vm.CloseDialogCommand.CanExecute(null))
                {
                    vm.CloseDialogCommand.Execute(null);
                }
            }
        }

        /// <summary>
        /// Detiene la propagación del evento clic cuando se pulsa DENTRO de la tarjeta.
        /// Esto evita que el Overlay_MouseDown se dispare y cierre el diálogo accidentalmente.
        /// </summary>
        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}