using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Reclamos.Services.ObtenerDetalleReclamoHistoricoService;

namespace Japdeva.APIMovil.Reclamos.Controllers
{
    /// <summary>
    /// Controlador para exponer el detalle histórico de un reclamo.
    /// </summary>

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DetalleReclamoHistoricoController : Controller
    {
        /// <summary>
        /// Obtiene el detalle histórico de un reclamo específico.
        /// </summary>
        /// <param name="obtenerDetalleReclamoService">Servicio para obtener el detalle histórico del reclamo.</param>
        /// <param name="idDetalleReclamo">Identificador del reclamo a consultar.</param>
        /// <param name="pagina">Número de página para la paginación de resultados.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        [HttpGet("ObtenerDetalleReclamoHistorico")]
        public Task<IActionResult> ObtenerDetalleReclamoHistorico([FromServices] IObtenerDetalleReclamoHistoricoService obtenerDetalleReclamoService,
        [FromQuery(Name = "id-reclamo")] long idDetalleReclamo, [FromQuery(Name = "pagina")] int pagina) =>
        obtenerDetalleReclamoService.ObtenerDetalleReclamoAsync(HttpContext.TraceIdentifier, idDetalleReclamo, pagina);
    }
}
