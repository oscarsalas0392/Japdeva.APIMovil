using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Usuarios.Services.ObtenerRolesService;

namespace Japdeva.APIMovil.Usuarios.Controllers
{
    /// <summary>
    /// Controlador para la consulta de roles del sistema.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RolController : Controller
    {
        /// <summary>
        /// Obtiene todos los roles activos.
        /// </summary>
        /// <param name="obtenerRolesService">Servicio para obtener roles.</param>
        /// <returns>Lista de roles activos.</returns>
        [HttpGet("ObtenerTodosLosRoles")]
        public Task<IActionResult> ObtenerTodosLosRolesAsync([FromServices] IObtenerRolesService obtenerRolesService) =>
            obtenerRolesService.ObtenerTodosLosRolesAsync(HttpContext.TraceIdentifier);

        /// <summary>
        /// Obtiene un rol por su identificador.
        /// </summary>
        /// <param name="obtenerRolesService">Servicio para obtener roles.</param>
        /// <param name="id">Identificador del rol.</param>
        /// <returns>Datos del rol encontrado.</returns>
        [HttpGet("ObtenerRolPorId")]
        public Task<IActionResult> ObtenerRolPorIdAsync([FromServices] IObtenerRolesService obtenerRolesService, [FromQuery(Name = "id")] int id) =>
            obtenerRolesService.ObtenerRolPorIdAsync(HttpContext.TraceIdentifier, id);
    }
}
