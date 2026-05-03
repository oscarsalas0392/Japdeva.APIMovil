using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Usuarios.Models;
using Japdeva.APIMovil.Usuarios.Services.AutenticarUsuarioService;
using Japdeva.APIMovil.Usuarios.Services.OlvidarContrasenaService;

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

        /// <summary>
        /// Genera una contraseña temporal para el usuario identificado por correo.
        /// No requiere autenticación.
        /// </summary>
        /// <param name="olvidarContrasenaService">Servicio de recuperación de contraseña.</param>
        /// <param name="solicitud">Correo del usuario que olvidó su contraseña.</param>
        /// <returns>Respuesta genérica para evitar enumeración de usuarios.</returns>
        [AllowAnonymous]
        [HttpPost("OlvidarContrasena")]
        public Task<IActionResult> OlvidarContrasenaAsync(
            [FromServices] IOlvidarContrasenaService olvidarContrasenaService,
            [FromBody] OlvidarContrasenaSolicitudModel solicitud) =>
            olvidarContrasenaService.OlvidarContrasenaAsync(HttpContext.TraceIdentifier, solicitud);
    }
}
