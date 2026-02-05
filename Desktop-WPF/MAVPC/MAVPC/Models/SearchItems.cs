namespace MAVPC.Models
{
    // Clase auxiliar para la lista desplegable del buscador
    public class SearchItem
    {
        public string Titulo { get; set; } = "";
        public string Subtitulo { get; set; } = "";
        public string Icono { get; set; } = "Magnify"; // Nombre del icono Material
        public string Color { get; set; } = "White";

        // Coordenadas para viajar al hacer clic
        public double Lat { get; set; }
        public double Lon { get; set; }

        // Guardamos el objeto original por si acaso
        public object? DataObject { get; set; }
    }
}

