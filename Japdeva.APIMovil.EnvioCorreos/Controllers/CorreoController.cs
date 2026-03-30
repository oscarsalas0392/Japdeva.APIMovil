using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.EnvioCorreos.Models;
using Japdeva.APIMovil.EnvioCorreos.Services.AgregarCorreoService;

namespace Japdeva.APIMovil.EnvioCorreos.Controllers
{
    /// <summary>
    /// Controlador para la gestión de correos electrónicos en la cola de envío.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CorreoController : Controller
    {
        /// <summary>
        /// Registra un nuevo correo en la cola de envío.
        /// </summary>
        /// <param name="agregarCorreoService">Servicio para registrar correos.</param>
        /// <param name="solicitud">Modelo con los datos del correo a registrar.</param>
        /// <returns>Respuesta del registro del correo.</returns>
        [HttpPost("AgregarCorreo")]
        public Task<IActionResult> AgregarCorreoAsync([FromServices] IAgregarCorreoService agregarCorreoService, [FromBody] AgregarCorreoSolicitudModel solicitud) =>
            agregarCorreoService.AgregarCorreoAsync(HttpContext.TraceIdentifier, solicitud);
    }
}
