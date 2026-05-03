using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Reclamos.Services.ObtenerDocumentoUsuarioHistoricoService;


namespace Japdeva.APIMovil.Reclamos.Controllers
{
    /// <summary>
    /// Controlador para exponer los endpoints relacionados con el histórico de documentos de usuario.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentoUsuarioHistoricoController : Controller
    {
       /// <summary>
       /// Obtiene el histórico de documentos asociados a un usuario para un reclamo específico y página indicada.
       /// </summary>
       /// <param name="obtenerDocumentoUsuarioService">Servicio para obtener el histórico de documentos de usuario.</param>
       /// <param name="idReclamo">Identificador del reclamo.</param>
       /// <param name="pagina">Número de página a consultar.</param>
       /// <returns>Una acción que representa el resultado de la operación.</returns>
       [HttpGet]
       public Task<IActionResult> ObtenerDocumentos([FromServices] IObtenerDocumentoUsuarioHistoricoService obtenerDocumentoUsuarioService,
       [FromQuery(Name = "id-reclamo")] long idReclamo, [FromQuery(Name = "pagina")] int pagina) =>
       obtenerDocumentoUsuarioService.ObtenerDocumentoUsuarioAsync(HttpContext.TraceIdentifier, idReclamo, pagina);
    }
}
