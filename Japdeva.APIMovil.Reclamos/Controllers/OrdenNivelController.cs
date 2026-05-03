using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Reclamos.Services.ObtenerOrdenNivelProcesoService;

namespace Japdeva.APIMovil.Reclamos.Controllers
{
    /// <summary>
    /// Controlador para gestionar las operaciones relacionadas con los niveles de orden.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdenNivelController : Controller
    {

        /// <summary>
        /// Obtiene los niveles de orden según el identificador del nivel superior y la página solicitada.
        /// </summary>
        /// <param name="obtenerOrdenNivelProcesoService">Servicio para obtener el proceso de orden de nivel.</param>
        /// <param name="idNivelSuperior">Identificador del nivel superior.</param>
        /// <param name="pagina">Número de página solicitada.</param>
        /// <returns>Una acción HTTP con el resultado de la operación.</returns>
        [HttpGet("ObtenerOrdenNiveles")]
        public Task<IActionResult> ObtenerOrdenNiveles([FromServices] IObtenerOrdenNivelProcesoService obtenerOrdenNivelProcesoService,
            [FromQuery(Name ="id-nivel-superior")] int idNivelSuperior, [FromQuery(Name = "pagina")] int pagina ) =>
            obtenerOrdenNivelProcesoService.ObtenerOrdenNivelProcesoAsync(HttpContext.TraceIdentifier, idNivelSuperior, pagina);
    }
}
