using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Reclamos.Services.ObtenerDocumentoInternoHistoricoService;


namespace Japdeva.APIMovil.Reclamos.Controllers
{
    /// <summary>
    /// Controlador para gestionar la obtención de documentos internos históricos asociados a reclamos.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentoInternoHistoricoController : Controller
    {
        /// <summary>
        /// Obtiene los documentos internos asociados a un reclamo específico.
        /// </summary>
        /// <param name="obtenerDocumentoInternoService">Servicio para obtener documentos internos.</param>
        /// <param name="idReclamoDetalle">Identificador del detalle del reclamo.</param>
        /// <param name="pagina">Número de la página de resultados a obtener.</param>
        /// <returns>Resultado de la operación de obtención de documentos internos.</returns>
        [HttpGet("ObtenerDocumentoInternoPorReclamo")]
        public Task<IActionResult> ObtenerDocumentoInternoPorReclamo([FromServices] IObtenerDocumentoInternoHistoricoService obtenerDocumentoInternoService,
            [FromQuery(Name = "id-reclamo-detalle")] long idReclamoDetalle, [FromQuery(Name = "pagina")] int pagina) =>
            obtenerDocumentoInternoService.ObtenerDocumentosInternosAsync(HttpContext.TraceIdentifier, idReclamoDetalle, pagina);
    }
}
