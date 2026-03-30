using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Usuarios.Models;
using Japdeva.APIMovil.Usuarios.Services.AgregarDepartamentoUsuarioService;
using Japdeva.APIMovil.Usuarios.Services.EliminarDepartamentoUsuarioService;
using Japdeva.APIMovil.Usuarios.Services.ObtenerDepartamentosUsuariosService;

namespace Japdeva.APIMovil.Usuarios.Controllers
{
    /// <summary>
    /// Controlador para la gestión de asignaciones de usuarios a departamentos.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DepartamentoUsuarioController : Controller
    {
        /// <summary>
        /// Asigna un usuario a un departamento.
        /// </summary>
        /// <param name="agregarDepartamentoUsuarioService">Servicio para asignar usuarios a departamentos.</param>
        /// <param name="solicitud">Datos de la asignación.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpPost("AgregarDepartamentoUsuario")]
        public Task<IActionResult> AgregarDepartamentoUsuarioAsync([FromServices] IAgregarDepartamentoUsuarioService agregarDepartamentoUsuarioService, [FromBody] AgregarDepartamentoUsuarioSolicitudModel solicitud) =>
            agregarDepartamentoUsuarioService.AgregarDepartamentoUsuarioAsync(HttpContext.TraceIdentifier, solicitud);

        /// <summary>
        /// Obtiene los departamentos asignados a un usuario.
        /// </summary>
        /// <param name="obtenerDepartamentosUsuariosService">Servicio para obtener departamentos del usuario.</param>
        /// <param name="idUsuario">Identificador del usuario.</param>
        /// <param name="pagina">Número de página.</param>
        /// <returns>Lista de asignaciones de departamentos.</returns>
        [HttpGet("ObtenerDepartamentosPorUsuario")]
        public Task<IActionResult> ObtenerDepartamentosPorUsuarioAsync([FromServices] IObtenerDepartamentosUsuariosService obtenerDepartamentosUsuariosService,
            [FromQuery(Name = "id-usuario")] int idUsuario, [FromQuery(Name = "pagina")] int pagina) =>
            obtenerDepartamentosUsuariosService.ObtenerDepartamentosPorUsuarioAsync(HttpContext.TraceIdentifier, idUsuario, pagina);

        /// <summary>
        /// Elimina la asignación de un usuario a un departamento.
        /// </summary>
        /// <param name="eliminarDepartamentoUsuarioService">Servicio para eliminar asignaciones.</param>
        /// <param name="id">Identificador de la asignación a eliminar.</param>
        /// <returns>Resultado de la operación.</returns>
        [HttpDelete("EliminarDepartamentoUsuario")]
        public Task<IActionResult> EliminarDepartamentoUsuarioAsync([FromServices] IEliminarDepartamentoUsuarioService eliminarDepartamentoUsuarioService, [FromQuery(Name = "id")] long id) =>
            eliminarDepartamentoUsuarioService.EliminarDepartamentoUsuarioAsync(HttpContext.TraceIdentifier, id);
    }
}
