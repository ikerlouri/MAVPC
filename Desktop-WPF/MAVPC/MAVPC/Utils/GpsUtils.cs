using System;
using System.Globalization;

namespace MAVPC.Utils
{
    /// <summary>
    /// Utilidades para cálculos geoespaciales y conversión de coordenadas.
    /// Soporta WGS84 y conversión UTM (Zona 30N por defecto).
    /// </summary>
    public static class GpsUtils
    {
        // Constantes del elipsoide WGS84
        private const double A = 6378137;           // Radio ecuatorial (Semimajor axis)
        private const double E = 0.081819191;       // Excentricidad
        private const double K0 = 0.9996;           // Factor de escala
        private const double DegToRad = Math.PI / 180.0;
        private const double RadToDeg = 180.0 / Math.PI;

        /// <summary>
        /// Método inteligente que intenta detectar el formato de coordenadas (Decimal o UTM)
        /// y devuelve siempre Latitud/Longitud decimales listas para el mapa.
        /// </summary>
        /// <param name="latStr">Latitud o coordenada Y UTM en string</param>
        /// <param name="lonStr">Longitud o coordenada X UTM en string</param>
        /// <returns>Tupla (Lat, Lon) normalizada. Devuelve (0,0) si falla.</returns>
        public static (double Lat, double Lon) NormalizeCoordinates(string? latStr, string? lonStr)
        {
            if (string.IsNullOrWhiteSpace(latStr) || string.IsNullOrWhiteSpace(lonStr))
                return (0, 0);

            // 1. Parseo seguro (Culture Invariant maneja puntos y comas mejor)
            // Reemplazamos coma por punto para asegurar compatibilidad universal
            if (!double.TryParse(latStr.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double c1) ||
                !double.TryParse(lonStr.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double c2))
            {
                return (0, 0);
            }

            // 2. Heurística de detección de formato
            // Las latitudes decimales van de -90 a 90.
            // Las coordenadas UTM son valores enormes (ej. 4700000).
            // Si el valor absoluto es mayor a 180, asumimos UTM.
            if (Math.Abs(c1) > 90 || Math.Abs(c2) > 180)
            {
                // En UTM: c1 suele ser Y (Norte/Latitud) y c2 X (Este/Longitud)
                // OJO: Depende de tu API. Si vienen invertidas, cambia el orden aquí: UtmToLatLng(c2, c1)
                // Asumimos orden estándar: Lat field -> Y, Lon field -> X
                return UtmToLatLng(c2, c1);
            }

            // Ya son decimales
            return (c1, c2);
        }

        /// <summary>
        /// Convierte coordenadas UTM (X, Y) a Latitud/Longitud (WGS84).
        /// </summary>
        public static (double Lat, double Lon) UtmToLatLng(double x, double y, int zone = 30, bool north = true)
        {
            try
            {
                // Validación de rango básico para evitar cálculos NaN
                if (x < 100000 || x > 900000) return (0, 0);

                // 1. Cálculos preliminares
                double e2 = E * E;
                double e1 = (1 - Math.Sqrt(1 - e2)) / (1 + Math.Sqrt(1 - e2));

                // Optimizaciones de potencias (multiplicación directa es más rápida que Math.Pow)
                double e1_2 = e1 * e1;
                double e1_3 = e1_2 * e1;
                double e1_4 = e1_2 * e1_2;

                // 2. Ajuste de coordenadas (Falsos Norte/Este)
                double yAdjusted = north ? y : y - 10000000.0;
                double xAdjusted = x - 500000.0;

                // 3. Latitud Footprint
                double m = yAdjusted / K0;
                double mu = m / (A * (1 - e2 / 4 - 3 * e2 * e2 / 64 - 5 * e2 * e2 * e2 / 256));

                double phi1Rad = mu + (3 * e1 / 2 - 27 * e1_3 / 32) * Math.Sin(2 * mu)
                                    + (21 * e1_2 / 16 - 55 * e1_4 / 32) * Math.Sin(4 * mu)
                                    + (151 * e1_3 / 96) * Math.Sin(6 * mu);

                // 4. Variables auxiliares
                double sinPhi1 = Math.Sin(phi1Rad);
                double cosPhi1 = Math.Cos(phi1Rad);
                double tanPhi1 = Math.Tan(phi1Rad);

                double n1 = A / Math.Sqrt(1 - e2 * sinPhi1 * sinPhi1);
                double t1 = tanPhi1 * tanPhi1;
                double t1_2 = t1 * t1;

                double r1 = A * (1 - e2) / Math.Pow(1 - e2 * sinPhi1 * sinPhi1, 1.5);
                double d = xAdjusted / (n1 * K0);

                // Potencias de D
                double d2 = d * d;
                double d3 = d2 * d;
                double d4 = d2 * d2;
                double d5 = d4 * d;
                double d6 = d3 * d3;

                // 5. Cálculo Latitud
                double term1 = d2 / 2;
                double term2 = (5 + 3 * t1 + 10 * C(cosPhi1, e2) - 4 * Math.Pow(C(cosPhi1, e2), 2) - 9 * e2 * C(cosPhi1, e2)) * d4 / 24;
                double term3 = (61 + 90 * t1 + 298 * C(cosPhi1, e2) + 45 * t1_2 - 252 * e2 * C(cosPhi1, e2) - 3 * Math.Pow(C(cosPhi1, e2), 2)) * d6 / 720;

                double latRad = phi1Rad - (n1 * tanPhi1 / r1) * (term1 - term2 + term3);

                // 6. Cálculo Longitud
                double lTerm1 = d;
                double lTerm2 = (1 + 2 * t1 + C(cosPhi1, e2)) * d3 / 6;
                double lTerm3 = (5 - 2 * C(cosPhi1, e2) + 28 * t1 - 3 * Math.Pow(C(cosPhi1, e2), 2) + 8 * e2 * C(cosPhi1, e2) + 24 * t1_2) * d5 / 120;

                double lonRad = (lTerm1 - lTerm2 + lTerm3) / cosPhi1;

                // Ajuste meridiano central
                double centralMeridianRad = ((zone * 6 - 180) - 3) * DegToRad;

                return (latRad * RadToDeg, (lonRad + centralMeridianRad) * RadToDeg);
            }
            catch
            {
                // En caso de error matemático, devolvemos 0,0 para no romper la UI
                return (0, 0);
            }
        }

        // Helper para simplificar la fórmula visualmente
        private static double C(double cosPhi, double e2)
        {
            return e2 * cosPhi * cosPhi / (1 - e2);
        }
    }
}