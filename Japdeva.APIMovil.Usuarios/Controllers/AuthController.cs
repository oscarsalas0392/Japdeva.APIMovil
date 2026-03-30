using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Usuarios.Models;
using Japdeva.APIMovil.Usuarios.Services.AutenticarUsuarioService;

namespace Japdeva.APIMovil.Usuarios.Controllers
{
    /// <summary>
    /// Controlador para la autenticación de usuarios del sistema.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        /// <summary>
        /// Verifica las credenciales del usuario y devuelve sus datos si son válidas.
        /// </summary>
        /// <param name="autenticarUsuarioService">Servicio de autenticación de usuarios.</param>
        /// <param name="solicitud">Credenciales del usuario.</param>
        /// <returns>Datos del usuario autenticado o error 401 si las credenciales son inválidas.</returns>
        [HttpPost("Autenticar")]
        public Task<IActionResult> AutenticarAsync(
            [FromServices] IAutenticarUsuarioService autenticarUsuarioService,
            [FromBody] AutenticarUsuarioSolicitudModel solicitud) =>
            autenticarUsuarioService.AutenticarAsync(HttpContext.TraceIdentifier, solicitud);
    }
}
