using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.AgregarDocumentoInternoService;
using Japdeva.APIMovil.Reclamos.Services.EliminarDocumentoInternoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerDocumentoInternoPorIdReclamoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerDocumentoInternoService;

namespace Japdeva.APIMovil.Reclamos.Controllers
{
    /// <summary>
    /// Controlador para gestionar documentos internos asociados a reclamos.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentoInternoController : Controller
    {
        /// <summary>
        /// Agrega un documento interno asociado a un reclamo.
        /// </summary>
        /// <param name="agregarDocumentoInternoService">Servicio para agregar documentos internos.</param>
        /// <param name="agregarDocumentoInterno">Modelo de solicitud con los datos del documento a agregar.</param>
        /// <returns>Resultado de la operación de agregar documento interno.</returns>
        [HttpPost("AgregarDocumentoInterno")]  
        public Task<IActionResult> AgregarDocumentoInterno([FromServices] IAgregarDocumentoInternoService agregarDocumentoInternoService,
            [FromBody] AgregarDocumentoInternoSolicitudModel agregarDocumentoInterno) =>
            agregarDocumentoInternoService.AgregarDocumentoInternoAsync(HttpContext.TraceIdentifier, agregarDocumentoInterno);


        /// <summary>
        /// Elimina un documento interno asociado a un reclamo.
        /// </summary>
        /// <param name="eliminarDocumentoInternoService">Servicio para eliminar documentos internos.</param>
        /// <param name="idDocumento">Identificador del documento a eliminar.</param>
        /// <returns>Resultado de la operación de eliminación del documento interno.</returns>
        [HttpDelete("EliminarDocumentoInterno")]
        public Task<IActionResult> EliminarDocumentoInterno([FromServices] IEliminarDocumentoInternoService eliminarDocumentoInternoService, [FromQuery(Name = "id-documento")] long idDocumento) =>
            eliminarDocumentoInternoService.EliminarDocumentoInternoAsync(HttpContext.TraceIdentifier, idDocumento);


        /// <summary>
        /// Obtiene los documentos internos asociados a un reclamo específico.
        /// </summary>
        /// <param name="obtenerDocumentoInternoService">Servicio para obtener documentos internos.</param>
        /// <param name="idReclamoDetalle">Identificador del detalle del reclamo.</param>
        /// <param name="pagina">Número de la página de resultados a obtener.</param>
        /// <returns>Resultado de la operación de obtención de documentos internos.</returns>
        [HttpGet("ObtenerDocumentoInternoPorReclamo")]
        public Task<IActionResult> ObtenerDocumentoInternoPorReclamo([FromServices] IObtenerDocumentoInternoService obtenerDocumentoInternoService,
            [FromQuery(Name = "id-reclamo-detalle")] long idReclamoDetalle, [FromQuery(Name = "pagina")] int pagina) =>
            obtenerDocumentoInternoService.ObtenerDocumentosInternosAsync(HttpContext.TraceIdentifier, idReclamoDetalle, pagina);

        /// <summary>
        /// Obtiene los documentos internos de todos los detalles de un reclamo,
        /// incluyendo la descripción del detalle y el nombre del departamento.
        /// </summary>
        /// <param name="obtenerDocumentoInternoPorIdReclamoService">Servicio para obtener los documentos por ID de reclamo.</param>
        /// <param name="idReclamo">Identificador del reclamo a consultar.</param>
        /// <param name="pagina">Número de página para la paginación de resultados.</param>
        /// <returns>Lista paginada de documentos internos del reclamo indicado.</returns>
        [HttpGet("ObtenerDocumentoInternoPorIdReclamo")]
        public Task<IActionResult> ObtenerDocumentoInternoPorIdReclamo(
            [FromServices] IObtenerDocumentoInternoPorIdReclamoService obtenerDocumentoInternoPorIdReclamoService,
            [FromQuery(Name = "id-reclamo")] long idReclamo,
            [FromQuery(Name = "pagina")] int pagina) =>
            obtenerDocumentoInternoPorIdReclamoService.ObtenerDocumentoInternoPorIdReclamoAsync(HttpContext.TraceIdentifier, idReclamo, pagina);
    }
}
