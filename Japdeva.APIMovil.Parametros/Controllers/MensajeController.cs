using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Parametros.Services.ObtenerMensajePorPantallaService;

namespace Japdeva.APIMovil.Parametros.Controllers
{
    /// <summary>
    /// Controlador para gestionar las operaciones relacionadas con los mensajes.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MensajeController : Controller
    {
        /// <summary>
        /// Obtiene los mensajes activos asociados a una pantalla específica.
        /// </summary>
        /// <param name="obtenerMensajePorPantallaService">Servicio para obtener los mensajes por pantalla.</param>
        /// <param name="idPantalla">Identificador de la pantalla para filtrar los mensajes.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        [HttpGet("ObtenerMensajesPorPantalla")]
        public Task<IActionResult> ObtenerMensajesPorPantalla(
            [FromServices] IObtenerMensajePorPantallaService obtenerMensajePorPantallaService,
            [FromQuery(Name = "id-pantalla")] int idPantalla) =>
            obtenerMensajePorPantallaService.ObtenerMensajesPorPantallaAsync(HttpContext.TraceIdentifier, idPantalla);
    }
}
