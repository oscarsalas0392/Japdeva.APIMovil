using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EditarReclamoDetalleService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerDetalleReclamoPorDepartamentoEstadoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerDetalleReclamoPorIdDetalleService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerDetalleReclamoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerDocumentoInternoPorIdReclamoService;


namespace Japdeva.APIMovil.Reclamos.Controllers
{
    /// <summary>
    /// Controlador para gestionar las operaciones relacionadas con el detalle de reclamo.
    /// </summary>
    [Authorize]
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

        /// <summary>
        /// Obtiene un detalle de reclamo específico por su identificador único.
        /// </summary>
        /// <param name="obtenerDetalleReclamoPorIdDetalleService">Servicio para obtener el detalle por ID.</param>
        /// <param name="idDetalleReclamo">Identificador único del detalle de reclamo a consultar.</param>
        /// <returns>El detalle de reclamo correspondiente al identificador indicado.</returns>
        [HttpGet("ObtenerDetalleReclamoPorIdDetalle")]
        public Task<IActionResult> ObtenerDetalleReclamoPorIdDetalle(
            [FromServices] IObtenerDetalleReclamoPorIdDetalleService obtenerDetalleReclamoPorIdDetalleService,
            [FromQuery(Name = "id-detalle")] long idDetalleReclamo) =>
            obtenerDetalleReclamoPorIdDetalleService.ObtenerDetalleReclamoPorIdDetalleAsync(HttpContext.TraceIdentifier, idDetalleReclamo);

        /// <summary>
        /// Obtiene los detalles de reclamo filtrados por departamento y estado detalle de forma paginada.
        /// </summary>
        /// <param name="obtenerDetalleReclamoPorDepartamentoEstadoService">Servicio para obtener los detalles filtrados.</param>
        /// <param name="idDepartamento">Identificador del departamento a filtrar.</param>
        /// <param name="idEstadoDetalleReclamo">Identificador del estado detalle de reclamo a filtrar.</param>
        /// <param name="idReclamo">Identificador del reclamo a filtrar.</param>
        /// <returns>El último detalle de reclamo que coincide con los filtros indicados.</returns>
        [HttpGet("ObtenerDetalleReclamoPorIdDepartamentoYIdEstadoDetalle")]
        public Task<IActionResult> ObtenerDetalleReclamoPorIdDepartamentoYIdEstadoDetalle(
            [FromServices] IObtenerDetalleReclamoPorDepartamentoEstadoService obtenerDetalleReclamoPorDepartamentoEstadoService,
            [FromQuery(Name = "id-departamento")] long idDepartamento,
            [FromQuery(Name = "id-estado-detalle")] int idEstadoDetalleReclamo,
            [FromQuery(Name = "id-reclamo")] long idReclamo) =>
            obtenerDetalleReclamoPorDepartamentoEstadoService.ObtenerDetalleReclamoPorDepartamentoEstadoAsync(HttpContext.TraceIdentifier, idDepartamento, idEstadoDetalleReclamo, idReclamo);

        /// <summary>
        /// Obtiene el expediente digital de un reclamo: todos sus detalles con departamento,
        /// descripción, fechas de inicio y fin, usuario que atendió y documento adjunto si existe.
        /// </summary>
        /// <param name="obtenerDocumentoInternoPorIdReclamoService">Servicio para obtener el expediente.</param>
        /// <param name="idReclamo">Identificador del reclamo a consultar.</param>
        /// <param name="pagina">Número de página para la paginación de resultados.</param>
        /// <returns>Lista paginada del expediente digital del reclamo indicado.</returns>
        [HttpGet("ObtenerExpedientePorReclamo")]
        public Task<IActionResult> ObtenerExpedientePorReclamo(
            [FromServices] IObtenerDocumentoInternoPorIdReclamoService obtenerDocumentoInternoPorIdReclamoService,
            [FromQuery(Name = "id-reclamo")] long idReclamo,
            [FromQuery(Name = "pagina")] int pagina) =>
            obtenerDocumentoInternoPorIdReclamoService.ObtenerDocumentoInternoPorIdReclamoAsync(HttpContext.TraceIdentifier, idReclamo, pagina);
    }
}
