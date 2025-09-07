namespace Japdeva.APIMovil.Usuarios
{
    /// <summary>
    /// Representa un pronóstico meteorológico con temperatura y descripción
    /// </summary>
    public class WeatherForecast
    {
        private const int CELSIUS_TO_FAHRENHEIT_OFFSET = 32;
        private const double CELSIUS_TO_FAHRENHEIT_RATIO = 0.5556;

        /// <summary>
        /// Fecha del pronóstico meteorológico
        /// </summary>
        public DateOnly Date { get; set; }
        /// <summary>
        /// Temperatura en grados Celsius
        /// </summary>
        public int TemperatureC { get; set; }
        /// <summary>
        /// Temperatura en grados Fahrenheit calculada automáticamente
        /// </summary>
        public int TemperatureF => CELSIUS_TO_FAHRENHEIT_OFFSET + (int)(TemperatureC / CELSIUS_TO_FAHRENHEIT_RATIO);

        /// <summary>
        /// Descripción textual del estado del tiempo
        /// </summary>
        public string? Summary { get; set; }
    }
}
