using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EditarReclamoDetalleService;


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
    }
}
