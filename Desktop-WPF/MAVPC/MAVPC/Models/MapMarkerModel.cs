namespace MAVPC.Models
{
    public class MapMarkerModel
    {
        // Usamos nombres cortos porque así lo espera el JavaScript que te pasé
        public double Lat { get; set; }
        public double Lon { get; set; }

        // Tipos aceptados por el JS: "camara", "incidencia", "obra"
        public string Type { get; set; } = "incidencia";

        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Propiedad opcional para guardar tu objeto original (Cámara o Incidencia real)
        // por si luego quieres hacer cosas avanzadas al recibir clics.
        public object? DataObject { get; set; }
    }
}

