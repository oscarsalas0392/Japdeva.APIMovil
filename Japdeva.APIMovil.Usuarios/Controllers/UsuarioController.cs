using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Usuarios.Models;
using Japdeva.APIMovil.Usuarios.Services.ActualizarUsuarioService;
using Japdeva.APIMovil.Usuarios.Services.AgregarUsuarioService;
using Japdeva.APIMovil.Usuarios.Services.EliminarUsuarioService;
using Japdeva.APIMovil.Usuarios.Services.ObtenerUsuariosService;

namespace Japdeva.APIMovil.Usuarios.Controllers
{
    /// <summary>
    /// Controlador para la gesti�n de usuarios.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : Controller
    {
        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="agregarUsuarioService">Servicio para registrar usuarios.</param>
        /// <param name="solicitud">Modelo con los datos del usuario a registrar.</param>
        /// <returns>Respuesta del registro del usuario.</returns>
        [AllowAnonymous]
        [HttpPost("AgregarUsuario")]
        public Task<IActionResult> AgregarUsuarioAsync([FromServices] IAgregarUsuarioService agregarUsuarioService, [FromBody] AgregarUsuarioSolicitudModel solicitud) =>
            agregarUsuarioService.AgregarUsuarioAsync(HttpContext.TraceIdentifier, solicitud);

        /// <summary>
        /// Actualiza un usuario existente.
        /// </summary>
        /// <param name="actualizarUsuarioService">Servicio para actualizar usuarios.</param>
        /// <param name="solicitud">Modelo con los datos del usuario a actualizar.</param>
        /// <returns>Respuesta de la actualizaci�n del usuario.</returns>
        [HttpPut("ActualizarUsuario")]
        public Task<IActionResult> ActualizarUsuarioAsync([FromServices] IActualizarUsuarioService actualizarUsuarioService, [FromBody] ActualizarUsuarioSolicitudModel solicitud) =>
            actualizarUsuarioService.ActualizarUsuarioAsync(HttpContext.TraceIdentifier, solicitud);

        /// <summary>
        /// Obtiene todos los usuarios activos.
        /// </summary>
        /// <param name="obtenerUsuariosService">Servicio para obtener usuarios.</param>
        /// <returns>Respuesta con la lista de usuarios.</returns>
        [HttpGet("ObtenerTodosLosUsuarios")]
        public Task<IActionResult> ObtenerTodosLosUsuariosAsync([FromServices] IObtenerUsuariosService obtenerUsuariosService) =>
            obtenerUsuariosService.ObtenerTodosLosUsuariosAsync(HttpContext.TraceIdentifier);

        /// <summary>
        /// Obtiene un usuario por su identificador.
        /// </summary>
        /// <param name="obtenerUsuariosService">Servicio para obtener usuarios.</param>
        /// <param name="id">El identificador del usuario a obtener.</param>
        /// <returns>Respuesta con el usuario encontrado.</returns>
        [HttpGet("ObtenerUsuarioPorId")]
        public Task<IActionResult> ObtenerUsuarioPorIdAsync([FromServices] IObtenerUsuariosService obtenerUsuariosService, [FromQuery(Name = "id")] int id) =>
            obtenerUsuariosService.ObtenerUsuarioPorIdAsync(HttpContext.TraceIdentifier, id);

        /// <summary>
        /// Elimina un usuario por su identificador.
        /// </summary>
        /// <param name="eliminarUsuarioService">Servicio para eliminar usuarios.</param>
        /// <param name="id">El identificador del usuario a eliminar.</param>
        /// <returns>Respuesta de la eliminaci�n del usuario.</returns>
        [HttpDelete("EliminarUsuario")]
        public Task<IActionResult> EliminarUsuarioAsync([FromServices] IEliminarUsuarioService eliminarUsuarioService, [FromQuery(Name = "id")] int id) =>
            eliminarUsuarioService.EliminarUsuarioAsync(HttpContext.TraceIdentifier, id);
    }
}
