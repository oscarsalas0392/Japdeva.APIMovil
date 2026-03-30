using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Usuarios.Services.ObtenerTiposCedulaService;

namespace Japdeva.APIMovil.Usuarios.Controllers
{
    /// <summary>
    /// Controlador para la consulta de tipos de cédula.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TipoCedulaController : Controller
    {
        /// <summary>
        /// Obtiene todos los tipos de cédula activos.
        /// </summary>
        /// <param name="obtenerTiposCedulaService">Servicio para obtener tipos de cédula.</param>
        /// <returns>Lista de tipos de cédula activos.</returns>
        [HttpGet("ObtenerTodosLosTiposCedula")]
        public Task<IActionResult> ObtenerTodosLosTiposCedulaAsync([FromServices] IObtenerTiposCedulaService obtenerTiposCedulaService) =>
            obtenerTiposCedulaService.ObtenerTodosLosTiposCedulaAsync(HttpContext.TraceIdentifier);

        /// <summary>
        /// Obtiene un tipo de cédula por su identificador.
        /// </summary>
        /// <param name="obtenerTiposCedulaService">Servicio para obtener tipos de cédula.</param>
        /// <param name="id">Identificador del tipo de cédula.</param>
        /// <returns>Datos del tipo de cédula encontrado.</returns>
        [HttpGet("ObtenerTipoCedulaPorId")]
        public Task<IActionResult> ObtenerTipoCedulaPorIdAsync([FromServices] IObtenerTiposCedulaService obtenerTiposCedulaService, [FromQuery(Name = "id")] int id) =>
            obtenerTiposCedulaService.ObtenerTipoCedulaPorIdAsync(HttpContext.TraceIdentifier, id);
    }
}
