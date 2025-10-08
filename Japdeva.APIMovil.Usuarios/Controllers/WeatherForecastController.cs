using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Usuarios.Entities;

namespace Japdeva.APIMovil.Usuarios.Controllers
{

    /// <summary>
    /// Controlador para obtener pronósticos meteorológicos.
    /// </summary>
    [ApiController]
    [Authorize(Roles = "Usuarios")]
    [Route("api/weatherForecast")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            PRUEBA
        };
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IAgregarRepository<UsuarioEntity> _agregarRepository;
        private const string PRUEBA = "Freezing";
        private const string DEFAULT_TRACE_ID = "traceId";
        private const string DEFAULT_USER_NAME = "Juan Perez";
        private const int DEFAULT_MAX_TEMPERATURE = 55;
        private const int DEFAULT_MIN_TEMPERATURE = -10;
        private const int DEFAULT_NUMBER_OF_DAYS = 5;
        private const int DEFAULT_START_INDEX = 1;
      
        /// <summary>
        /// Inicializa una nueva instancia del controlador WeatherForecast
        /// </summary>
        /// <param name="logger">Logger para registrar eventos del controlador</param>
        public WeatherForecastController(ILogger<WeatherForecastController> logger, IAgregarRepository<UsuarioEntity> agregarRepository)
        {
            _logger = logger;
            _agregarRepository = agregarRepository;
        }

        /// <summary>
        /// Obtiene una lista de pronósticos meteorológicos aleatorios
        /// </summary>
        /// <returns>Colección de pronósticos meteorológicos para los próximos días</returns>
        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            this._agregarRepository.AgregarAsync(DEFAULT_TRACE_ID, new UsuarioEntity { Nombre = DEFAULT_USER_NAME });

            var resultado = Enumerable.Range(DEFAULT_START_INDEX, DEFAULT_NUMBER_OF_DAYS).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(DEFAULT_MIN_TEMPERATURE, DEFAULT_MAX_TEMPERATURE),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            });

            return resultado.ToArray();
        }
    }
}
