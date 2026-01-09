using CommunityToolkit.Mvvm.ComponentModel;

namespace MAVPC.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        // Datos simulados para las tarjetas
        [ObservableProperty] private string _traficoNivel = "ALTO";
        [ObservableProperty] private int _camarasActivas = 124;
        [ObservableProperty] private int _incidentesHoy = 3;
        [ObservableProperty] private string _tiempoSistema = System.DateTime.Now.ToString("HH:mm");

        public DashboardViewModel()
        {
            // Aquí cargaríamos datos de la base de datos
        }
    }
}