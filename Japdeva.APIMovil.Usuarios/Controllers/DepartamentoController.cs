using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Usuarios.Services.ObtenerDepartamentosService;

namespace Japdeva.APIMovil.Usuarios.Controllers
{
    /// <summary>
    /// Controlador para la consulta de departamentos.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DepartamentoController : Controller
    {
        /// <summary>
        /// Obtiene todos los departamentos activos.
        /// </summary>
        /// <param name="obtenerDepartamentosService">Servicio para obtener departamentos.</param>
        /// <returns>Lista de departamentos activos.</returns>
        [HttpGet("ObtenerTodosLosDepartamentos")]
        public Task<IActionResult> ObtenerTodosLosDepartamentosAsync([FromServices] IObtenerDepartamentosService obtenerDepartamentosService) =>
            obtenerDepartamentosService.ObtenerTodosLosDepartamentosAsync(HttpContext.TraceIdentifier);

        /// <summary>
        /// Obtiene un departamento por su identificador.
        /// </summary>
        /// <param name="obtenerDepartamentosService">Servicio para obtener departamentos.</param>
        /// <param name="id">Identificador del departamento.</param>
        /// <returns>Datos del departamento encontrado.</returns>
        [HttpGet("ObtenerDepartamentoPorId")]
        public Task<IActionResult> ObtenerDepartamentoPorIdAsync([FromServices] IObtenerDepartamentosService obtenerDepartamentosService, [FromQuery(Name = "id")] int id) =>
            obtenerDepartamentosService.ObtenerDepartamentoPorIdAsync(HttpContext.TraceIdentifier, id);
    }
}
