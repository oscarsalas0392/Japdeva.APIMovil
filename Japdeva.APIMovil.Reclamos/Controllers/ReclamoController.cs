using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.AgregarReclamoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerReclamoPorDepartamentoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerReclamosPorFechaIngresoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerReclamosPorUsuarioOrdenadoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerReclamosPorUsuarioService;

namespace Japdeva.APIMovil.Reclamos.Controllers
{
    /// <summary>
    /// Controlador para gestionar las operaciones relacionadas con los reclamos.
    /// </summary>
    /// 
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReclamoController : Controller
    {
        /// <summary>
        /// Agrega un nuevo reclamo utilizando el servicio correspondiente.
        /// </summary>
        /// <param name="agregarReclamoService">Servicio para agregar reclamos.</param>
        /// <param name="reclamo">Modelo con los datos del reclamo a agregar.</param>
        /// <returns>Resultado de la operación de agregar reclamo.</returns>
        [HttpPost("AgregarReclamo")]
        public Task<IActionResult> AgregarReclamo([FromServices] IAgregarReclamoService agregarReclamoService, [FromBody] AgregarReclamoSolicitudModel reclamo) =>
            agregarReclamoService.AgregarReclamoAsync(HttpContext.TraceIdentifier, reclamo);


        /// <summary>
        /// Obtiene los reclamos asociados a un usuario específico, filtrados por estado y paginados.
        /// </summary>
        /// <param name="obtenerReclamosPorUsuarioService">Servicio para obtener los reclamos por usuario.</param>
        /// <param name="idUsuario">Identificador del usuario.</param>
        /// <param name="idEstadoReclamo">Identificador del estado del reclamo.</param>
        /// <param name="pagina">Número de la página a consultar.</param>
        /// <returns>Resultado de la operación de obtención de reclamos por usuario.</returns>
        [HttpGet("ObtenerReclamoPorUsuario")]
        public Task<IActionResult> ObtenerReclamoPorUsuario([FromServices] IObtenerReclamosPorUsuarioService obtenerReclamosPorUsuarioService,
            [FromQuery(Name = "id-usuario")] int idUsuario, [FromQuery(Name = "id-estado-reclamo")] int idEstadoReclamo, [FromQuery(Name = "pagina")] int pagina) =>
            obtenerReclamosPorUsuarioService.ObtenerReclamosPorUsuarioAsync(HttpContext.TraceIdentifier, idUsuario, idEstadoReclamo, pagina);


        /// <summary>
        /// Obtiene los reclamos asociados a un departamento específico, paginados.
        /// </summary>
        /// <param name="obtenerReclamoPorDepartamentoService">Servicio para obtener los reclamos por departamento.</param>
        /// <param name="idDepartamento">Identificador del departamento.</param>
        /// <param name="pagina">Número de la página a consultar.</param>
        /// <returns>Resultado de la operación de obtención de reclamos por departamento.</returns>
        [HttpGet("ObtenerReclamoPorDepartamento")]
        public Task<IActionResult> ObtenerReclamoPorDepartamento([FromServices] IObtenerReclamoPorDepartamentoService obtenerReclamoPorDepartamentoService,
            [FromQuery(Name = "id-departamento")] int idDepartamento,
            [FromQuery(Name = "pagina")] int pagina) =>
            obtenerReclamoPorDepartamentoService.ObtenerReclamosPorDepartamentoAsync(HttpContext.TraceIdentifier, idDepartamento, pagina);


        /// <summary>
        /// Obtiene los reclamos filtrados por un rango de fechas de ingreso y estado, paginados.
        /// </summary>
        /// <param name="obtenerReclamosPorFechaIngresoService">Servicio para obtener los reclamos por fecha de ingreso.</param>
        /// <param name="fechaInicio">Fecha de inicio del rango a consultar.</param>
        /// <param name="fechaFin">Fecha de fin del rango a consultar.</param>
        /// <param name="idEstadoReclamo">Identificador del estado del reclamo.</param>
        /// <param name="pagina">Número de la página a consultar.</param>
        /// <returns>Resultado de la operación de obtención de reclamos por fecha y estado.</returns>
        [HttpGet("ObtenerReclamoPorFechaEstado")]
        public Task<IActionResult> ObtenerReclamoPorFechaEstado([FromServices] IObtenerReclamosPorFechaIngresoService obtenerReclamosPorFechaIngresoService,
            [FromQuery(Name = "fecha-inicio")] DateTime fechaInicio, [FromQuery(Name = "fecha-fin")] DateTime? fechaFin, [FromQuery(Name = "id-estado-reclamo")] int idEstadoReclamo, [FromQuery(Name = "pagina")] int pagina) =>
            obtenerReclamosPorFechaIngresoService.ObtenerReclamosPorFechaIngresoAsync(HttpContext.TraceIdentifier, fechaInicio, fechaFin, idEstadoReclamo, pagina);

        /// <summary>
        /// Obtiene todos los reclamos de un usuario ordenados del más reciente al más antiguo, paginados.
        /// </summary>
        /// <param name="obtenerReclamosPorUsuarioOrdenadoService">Servicio para obtener los reclamos ordenados por fecha.</param>
        /// <param name="idUsuario">Identificador del usuario.</param>
        /// <param name="pagina">Número de la página a consultar.</param>
        /// <returns>Resultado de la operación de obtención de reclamos ordenados por fecha descendente.</returns>
        [HttpGet("ObtenerReclamoPorUsuarioOrdenado")]
        public Task<IActionResult> ObtenerReclamoPorUsuarioOrdenado(
            [FromServices] IObtenerReclamosPorUsuarioOrdenadoService obtenerReclamosPorUsuarioOrdenadoService,
            [FromQuery(Name = "id-usuario")] int idUsuario,
            [FromQuery(Name = "pagina")] int pagina) =>
            obtenerReclamosPorUsuarioOrdenadoService.ObtenerReclamosPorUsuarioOrdenadoAsync(HttpContext.TraceIdentifier, idUsuario, pagina);

    }
}
