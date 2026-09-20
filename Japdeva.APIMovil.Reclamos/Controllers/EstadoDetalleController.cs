using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Reclamos.Services.ObtenerEstadoDetalleReclamoService;

namespace Japdeva.APIMovil.Reclamos.Controllers
{
    /// <summary>
    /// Controlador para gestionar las operaciones relacionadas con los estados detalle de los reclamos.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EstadoDetalleController : Controller
    {
        /// <summary>
        /// Obtiene los estados detalle de un reclamo según el nivel y la página especificados.
        /// </summary>
        /// <param name="obtenerEstadoDetalleReclamoService">Servicio para obtener los estados detalle del reclamo.</param>
        /// <param name="idNivel">Identificador del nivel del proceso.</param>
        /// <param name="pagina">Número de página para la paginación de resultados.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        [HttpGet("ObtenerEstadosDetalleReclamo")]
        public Task<IActionResult> ObtenerEstadosDetalleReclamo([FromServices] IObtenerEstadoDetalleReclamoService obtenerEstadoDetalleReclamoService,
            [FromQuery(Name ="id-nivel")] int idNivel, [FromQuery(Name ="pagina")] int pagina) =>
            obtenerEstadoDetalleReclamoService.ObtenerEstadoDetalleReclamoAsync(HttpContext.TraceIdentifier, idNivel, pagina);
    }
}
