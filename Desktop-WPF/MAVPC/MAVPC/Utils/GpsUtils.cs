using System;

namespace MAVPC.Utils
{
    public static class GpsUtils
    {
        // Constantes del elipsoide WGS84 (El estándar mundial GPS)
        private const double A = 6378137; // Radio ecuatorial
        private const double E = 0.081819191; // Excentricidad
        private const double K0 = 0.9996; // Factor de escala

        /// <summary>
        /// Convierte coordenadas UTM (X, Y) a Latitud/Longitud.
        /// Asume por defecto Zona 30 Norte (Euskadi/España).
        /// </summary>
        public static (double Lat, double Lon) UtmToLatLng(double x, double y, int zone = 30, bool north = true)
        {
            // 1. Cálculos preliminares
            double e2 = E * E; // Excentricidad al cuadrado
            double e1 = (1 - Math.Sqrt(1 - e2)) / (1 + Math.Sqrt(1 - e2));
            double e1_2 = e1 * e1;
            double e1_3 = e1 * e1 * e1;
            double e1_4 = e1 * e1 * e1 * e1;

            // 2. Ajuste de coordenadas
            // Restamos el "Falso Norte" (0 en hemisferio norte, 10.000.000 en sur)
            double yAdjusted = north ? y : y - 10000000.0;
            // Restamos el "Falso Este" (500.000 metros)
            double xAdjusted = x - 500000.0;

            // 3. Calcular la Latitud "Footprint"
            double m = yAdjusted / K0;
            double mu = m / (A * (1 - e2 / 4 - 3 * e2 * e2 / 64 - 5 * e2 * e2 * e2 / 256));

            double phi1Rad = mu + (3 * e1 / 2 - 27 * e1_3 / 32) * Math.Sin(2 * mu)
                             + (21 * e1_2 / 16 - 55 * e1_4 / 32) * Math.Sin(4 * mu)
                             + (151 * e1_3 / 96) * Math.Sin(6 * mu);

            // 4. Variables auxiliares sobre phi1
            double sinPhi1 = Math.Sin(phi1Rad);
            double cosPhi1 = Math.Cos(phi1Rad);
            double tanPhi1 = Math.Tan(phi1Rad);

            double n1 = A / Math.Sqrt(1 - e2 * sinPhi1 * sinPhi1);
            double t1 = tanPhi1 * tanPhi1;
            double t1_2 = t1 * t1;

            double r1 = A * (1 - e2) / Math.Pow(1 - e2 * sinPhi1 * sinPhi1, 1.5);
            double d = xAdjusted / (n1 * K0);
            double d2 = d * d;
            double d3 = d * d * d;
            double d4 = d * d * d * d;
            double d5 = d * d * d * d * d;
            double d6 = d * d * d * d * d * d;

            // 5. Cálculo final de Latitud
            double latRad = phi1Rad - (n1 * tanPhi1 / r1) * (d2 / 2 - (5 + 3 * t1 + 10 * (E * E * Math.Cos(phi1Rad) * Math.Cos(phi1Rad) / (1 - e2)) - 4 * (E * E * Math.Cos(phi1Rad) * Math.Cos(phi1Rad) / (1 - e2)) * (E * E * Math.Cos(phi1Rad) * Math.Cos(phi1Rad) / (1 - e2)) - 9 * e2 * (E * E * Math.Cos(phi1Rad) * Math.Cos(phi1Rad) / (1 - e2))) * d4 / 24 + (61 + 90 * t1 + 298 * (E * E * Math.Cos(phi1Rad) * Math.Cos(phi1Rad) / (1 - e2)) + 45 * t1_2 - 252 * e2 * (E * E * Math.Cos(phi1Rad) * Math.Cos(phi1Rad) / (1 - e2)) - 3 * (E * E * Math.Cos(phi1Rad) * Math.Cos(phi1Rad) / (1 - e2)) * (E * E * Math.Cos(phi1Rad) * Math.Cos(phi1Rad) / (1 - e2))) * d6 / 720);

            // 6. Cálculo final de Longitud
            double lonRad = (d - (1 + 2 * t1 + (E * E * Math.Cos(phi1Rad) * Math.Cos(phi1Rad) / (1 - e2))) * d3 / 6 + (5 - 2 * (E * E * Math.Cos(phi1Rad) * Math.Cos(phi1Rad) / (1 - e2)) + 28 * t1 - 3 * (E * E * Math.Cos(phi1Rad) * Math.Cos(phi1Rad) / (1 - e2)) * (E * E * Math.Cos(phi1Rad) * Math.Cos(phi1Rad) / (1 - e2)) + 8 * e2 * (E * E * Math.Cos(phi1Rad) * Math.Cos(phi1Rad) / (1 - e2)) + 24 * t1_2) * d5 / 120) / cosPhi1;

            // Ajuste por el Meridiano Central de la zona
            // Zona 30 tiene meridiano central en -3 grados, Zona 31 en +3, etc.
            double centralMeridianRad = ((zone * 6 - 180) - 3) * (Math.PI / 180.0);

            // Convertimos a grados
            double lat = latRad * (180.0 / Math.PI);
            double lon = (lonRad + centralMeridianRad) * (180.0 / Math.PI);

            return (lat, lon);
        }
    }
}
============================================================
ARCHIVO: C:\Users\2dam3\Documents\Retos\MAVPC\Desktop-WPF\MAVPC\MAVPC\Utils\GpsUtils.cs
============================================================
