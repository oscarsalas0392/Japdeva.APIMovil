using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Parametros.Services.ObtenerParametroService;

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
    }
}
