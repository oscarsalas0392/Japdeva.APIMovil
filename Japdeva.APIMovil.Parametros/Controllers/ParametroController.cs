using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Parametros.Services.ObtenerParametroService;
using Japdeva.APIMovil.Parametros.Services.ObtenerParametrosService;

namespace Japdeva.APIMovil.Parametros.Controllers
{
    /// <summary>
    /// Controlador para gestionar las operaciones relacionadas con los parámetros del sistema.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ParametroController : Controller
    {
        /// <summary>
        /// Obtiene un parámetro activo del sistema por su nombre.
        /// </summary>
        /// <param name="obtenerParametroService">Servicio para obtener parámetros del sistema.</param>
        /// <param name="nombre">Nombre del parámetro a obtener.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        [HttpGet("ObtenerParametroPorNombre")]
        public Task<IActionResult> ObtenerParametroPorNombre(
            [FromServices] IObtenerParametroService obtenerParametroService,
            [FromQuery(Name = "nombre")] string nombre) =>
            obtenerParametroService.ObtenerParametroPorNombreAsync(HttpContext.TraceIdentifier, nombre);

        /// <summary>
        /// Obtiene múltiples parámetros del sistema en una sola consulta por sus nombres.
        /// Los nombres no encontrados son omitidos del resultado.
        /// </summary>
        /// <param name="obtenerParametrosService">Servicio para obtener múltiples parámetros.</param>
        /// <param name="nombres">Lista de nombres de los parámetros a obtener.</param>
        /// <returns>Lista de parámetros encontrados para los nombres indicados.</returns>
        [HttpGet("ObtenerParametrosPorNombres")]
        public Task<IActionResult> ObtenerParametrosPorNombres(
            [FromServices] IObtenerParametrosService obtenerParametrosService,
            [FromQuery(Name = "nombres")] List<string> nombres) =>
            obtenerParametrosService.ObtenerParametrosPorNombresAsync(HttpContext.TraceIdentifier, nombres);
    }
}
