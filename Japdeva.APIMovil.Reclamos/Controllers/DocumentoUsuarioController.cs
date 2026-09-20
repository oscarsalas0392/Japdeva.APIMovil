using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Japdeva.APIMovil.Reclamos.Services.ObtenerDocumentoUsuarioService;

namespace Japdeva.APIMovil.Reclamos.Controllers
{
    /// <summary>
    /// Controlador para exponer los endpoints relacionados con los documentos de usuario asociados a un reclamo.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentoUsuarioController : Controller
    {
        /// <summary>
        /// Obtiene los documentos de usuario asociados a un reclamo específico y página indicada.
        /// </summary>
        /// <param name="obtenerDocumentoUsuarioService">Servicio para obtener documentos de usuario.</param>
        /// <param name="idReclamo">Identificador del reclamo.</param>
        /// <param name="pagina">Número de página para la paginación de resultados.</param>
        /// <returns>Una acción que representa el resultado de la operación.</returns>
        [HttpGet]
        public Task<IActionResult> ObtenerDocumentos([FromServices] IObtenerDocumentoUsuarioService obtenerDocumentoUsuarioService, 
            [FromQuery(Name ="id-reclamo")] long idReclamo, [FromQuery(Name = "pagina")] int pagina)=>
            obtenerDocumentoUsuarioService.ObtenerDocumentoUsuarioAsync(HttpContext.TraceIdentifier, idReclamo, pagina);
    }
}
