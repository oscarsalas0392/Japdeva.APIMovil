using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Usuarios.Models;
using Japdeva.APIMovil.Usuarios.Services.ActualizarUsuarioRolService;
using Japdeva.APIMovil.Usuarios.Services.EliminarUsuarioRolService;
using Japdeva.APIMovil.Usuarios.Services.ObtenerUsuariosRolesService;

namespace Japdeva.APIMovil.Usuarios.Controllers
{
    /// <summary>
    /// Controlador para la gestión de roles asignados a usuarios.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioRolController : Controller
    {

        /// <summary>
        /// Obtiene los roles asignados a un usuario.
        /// </summary>
        /// <param name="obtenerUsuariosRolesService">Servicio para obtener roles del usuario.</param>
        /// <param name="idUsuario">Identificador del usuario.</param>
        /// <param name="pagina">Número de página.</param>
        /// <returns>Lista de asignaciones de roles.</returns>
        [HttpGet("ObtenerRolesPorUsuario")]
        public Task<IActionResult> ObtenerRolesPorUsuarioAsync([FromServices] IObtenerUsuariosRolesService obtenerUsuariosRolesService,
            [FromQuery(Name = "id-usuario")] int idUsuario, [FromQuery(Name = "pagina")] int pagina) =>
            obtenerUsuariosRolesService.ObtenerRolesPorUsuarioAsync(HttpContext.TraceIdentifier, idUsuario, pagina);

        /// <summary>
        /// Actualiza la asignación de un rol a un usuario.
        /// </summary>
        /// <param name="actualizarUsuarioRolService">Servicio para actualizar asignaciones de roles.</param>
        /// <param name="solicitud">Modelo con los datos de la asignación a actualizar.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpPut("ActualizarUsuarioRol")]
        public Task<IActionResult> ActualizarUsuarioRolAsync([FromServices] IActualizarUsuarioRolService actualizarUsuarioRolService, [FromBody] ActualizarUsuarioRolSolicitudModel solicitud) =>
            actualizarUsuarioRolService.ActualizarUsuarioRolAsync(HttpContext.TraceIdentifier, solicitud);

        /// <summary>
        /// Elimina la asignación de un rol a un usuario.
        /// </summary>
        /// <param name="eliminarUsuarioRolService">Servicio para eliminar asignaciones de roles.</param>
        /// <param name="id">Identificador de la asignación a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpDelete("EliminarUsuarioRol")]
        public Task<IActionResult> EliminarUsuarioRolAsync([FromServices] IEliminarUsuarioRolService eliminarUsuarioRolService, [FromQuery(Name = "id")] long id) =>
            eliminarUsuarioRolService.EliminarUsuarioRolAsync(HttpContext.TraceIdentifier, id);
    }
}
