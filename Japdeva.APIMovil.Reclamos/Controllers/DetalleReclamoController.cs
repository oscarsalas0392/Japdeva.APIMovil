using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EditarReclamoDetalleService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerDetalleReclamoService;


namespace Japdeva.APIMovil.Reclamos.Controllers
{
    /// <summary>
    /// Controlador para gestionar las operaciones relacionadas con el detalle de reclamo.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DetalleReclamoController : Controller
    {
        /// <summary>
        /// Edita el detalle de un reclamo existente.
        /// </summary>
        /// <param name="editarReclamoDetalleService">Servicio para editar el detalle del reclamo.</param>
        /// <param name="detalleReclamo">Modelo con los datos del detalle de reclamo a editar.</param>
        /// <returns>Resultado de la operación de edición del detalle de reclamo.</returns>
        [HttpPut("EditarDetalleReclamo")]
        public Task<IActionResult> EditarDetalleReclamo([FromServices] IEditarReclamoDetalleService editarReclamoDetalleService,
            [FromBody] EditarDetalleReclamoSolicitudModel detalleReclamo) =>
            editarReclamoDetalleService.EditarReclamoDetalleAsync(HttpContext.TraceIdentifier, detalleReclamo);

        /// <summary>
        /// Obtiene el detalle de un reclamo específico según el identificador y la página solicitada.
        /// </summary>
        /// <param name="obtenerDetalleReclamoService">Servicio para obtener el detalle del reclamo.</param>
        /// <param name="idDetalleReclamo">Identificador del detalle de reclamo a consultar.</param>
        /// <param name="pagina">Número de página para la paginación de los resultados.</param>
        /// <returns>Resultado de la operación de obtención del detalle de reclamo.</returns>
        [HttpGet("ObtenerDetalleReclamo")]
        public Task<IActionResult> ObtenerDetalleReclamo([FromServices] IObtenerDetalleReclamoService obtenerDetalleReclamoService,
            [FromQuery(Name = "id-reclamo")] long idDetalleReclamo, [FromQuery(Name = "pagina")] int pagina) =>
            obtenerDetalleReclamoService.ObtenerDetalleReclamoAsync(HttpContext.TraceIdentifier, idDetalleReclamo, pagina);
    }
}
