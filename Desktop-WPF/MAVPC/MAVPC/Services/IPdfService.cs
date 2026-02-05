using MAVPC.Models;
using System.Collections.Generic;

namespace MAVPC.Services
{
    /// <summary>
    /// Interfaz para el servicio de generación de reportes.
    /// Abstrae la lógica de creación de documentos PDF.
    /// </summary>
    public interface IPdfService
    {
        /// <summary>
        /// Genera un informe completo de incidencias incluyendo KPIs, gráficos y tablas.
        /// </summary>
        /// <param name="filePath">Ruta completa donde se guardará el archivo .pdf</param>
        /// <param name="incidencias">Lista de datos para procesar</param>
        void GenerateFullReport(string filePath, List<Incidencia> incidencias);
    }
}