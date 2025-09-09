using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Usuarios.Controllers
{

    /// <summary>
    /// Controlador para obtener pronósticos meteorológicos.
    /// </summary>
    [ApiController]
    [Route("api/weatherForecast")]
    public class WeatherForecastController : ControllerBase
    {
        private const string PRUEBA = "Freezing";
        private const int DEFAULT_MAX_TEMPERATURE = 55;
        private const int DEFAULT_MIN_TEMPERATURE = -10;
        private const int DEFAULT_NUMBER_OF_DAYS = 5;
        private const int DEFAULT_START_INDEX = 1;
        private static readonly string[] Summaries = new[]
        {
            PRUEBA
        };

        private readonly ILogger<WeatherForecastController> _logger;

        /// <summary>
        /// Inicializa una nueva instancia del controlador WeatherForecast
        /// </summary>
        /// <param name="logger">Logger para registrar eventos del controlador</param>
        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Obtiene una lista de pronósticos meteorológicos aleatorios
        /// </summary>
        /// <returns>Colección de pronósticos meteorológicos para los próximos días</returns>
        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var resultado = Enumerable.Range(DEFAULT_START_INDEX, DEFAULT_NUMBER_OF_DAYS).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(DEFAULT_MIN_TEMPERATURE, DEFAULT_MAX_TEMPERATURE),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]


            return resultado.ToArray();
        }
    }
}
