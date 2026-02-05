using MAVPC.Models;
using System.Collections.Generic;

namespace MAVPC.Services
{
    public interface IPdfService
    {
        void GenerateFullReport(string filePath, List<Incidencia> incidencias);
    }
}