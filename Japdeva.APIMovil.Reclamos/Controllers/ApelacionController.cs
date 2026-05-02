using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.AgregarApelacionReclamoService;
using Japdeva.APIMovil.Reclamos.Services.EditarApelacionReclamoDetalleService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerApelacionesPorDepartamentoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerApelacionesPorFechaEstadoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerApelacionesPorUsuarioService;

namespace Japdeva.APIMovil.Reclamos.Controllers
{
    /// <summary>
    /// Controlador para gestionar las operaciones relacionadas con las apelaciones de reclamos.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ApelacionController : Controller
    {
        /// <summary>
        /// Agrega una nueva apelación sobre un reclamo resuelto.
        /// </summary>
        /// <param name="agregarApelacionReclamoService">Servicio para agregar apelaciones.</param>
        /// <param name="apelacion">Modelo con los datos de la apelación a agregar.</param>
        /// <returns>Resultado de la operación de agregar apelación.</returns>
        /// <summary>
        /// Edita el detalle de una apelación, avanzando, devolviendo o finalizando el workflow según el estado indicado.
        /// </summary>
        /// <param name="editarApelacionReclamoDetalleService">Servicio para editar el detalle de apelación.</param>
        /// <param name="solicitud">Modelo con los datos de la edición.</param>
        /// <returns>Resultado de la operación con la información del detalle editado.</returns>
        [HttpPut("EditarDetalleApelacion")]
        public Task<IActionResult> EditarDetalleApelacion(
            [FromServices] IEditarApelacionReclamoDetalleService editarApelacionReclamoDetalleService,
            [FromBody] EditarDetalleApelacionReclamoSolicitudModel solicitud) =>
            editarApelacionReclamoDetalleService.EditarApelacionReclamoDetalleAsync(HttpContext.TraceIdentifier, solicitud);

        [HttpPost("AgregarApelacion")]
        public Task<IActionResult> AgregarApelacion(
            [FromServices] IAgregarApelacionReclamoService agregarApelacionReclamoService,
            [FromBody] AgregarApelacionReclamoSolicitudModel apelacion) =>
            agregarApelacionReclamoService.AgregarApelacionReclamoAsync(HttpContext.TraceIdentifier, apelacion);

        /// <summary>
        /// Obtiene las apelaciones activas asignadas a un departamento específico, paginadas.
        /// Se consideran activas las apelaciones en estado Pendiente o EnProceso.
        /// </summary>
        /// <param name="obtenerApelacionesPorDepartamentoService">Servicio para obtener las apelaciones por departamento.</param>
        /// <param name="idDepartamento">Identificador del departamento.</param>
        /// <param name="pagina">Número de la página a consultar.</param>
        /// <returns>Resultado de la operación de obtención de apelaciones activas por departamento.</returns>
        [HttpGet("ObtenerApelacionesPorDepartamento")]
        public Task<IActionResult> ObtenerApelacionesPorDepartamento(
            [FromServices] IObtenerApelacionesPorDepartamentoService obtenerApelacionesPorDepartamentoService,
            [FromQuery(Name = "id-departamento")] int idDepartamento,
            [FromQuery(Name = "pagina")] int pagina) =>
            obtenerApelacionesPorDepartamentoService.ObtenerApelacionesPorDepartamentoAsync(HttpContext.TraceIdentifier, idDepartamento, pagina);

        /// <summary>
        /// Obtiene las apelaciones filtradas por rango de fechas de ingreso y estado, paginadas.
        /// </summary>
        /// <param name="obtenerApelacionesPorFechaEstadoService">Servicio para obtener las apelaciones por fecha y estado.</param>
        /// <param name="fechaInicio">Fecha de inicio del rango a consultar.</param>
        /// <param name="fechaFin">Fecha de fin del rango a consultar.</param>
        /// <param name="idEstadoReclamo">Identificador del estado de la apelación.</param>
        /// <param name="pagina">Número de la página a consultar.</param>
        /// <returns>Resultado de la operación de obtención de apelaciones por fecha y estado.</returns>
        [HttpGet("ObtenerApelacionesPorFechaEstado")]
        public Task<IActionResult> ObtenerApelacionesPorFechaEstado(
            [FromServices] IObtenerApelacionesPorFechaEstadoService obtenerApelacionesPorFechaEstadoService,
            [FromQuery(Name = "fecha-inicio")] DateTime fechaInicio,
            [FromQuery(Name = "fecha-fin")] DateTime fechaFin,
            [FromQuery(Name = "id-estado-reclamo")] int idEstadoReclamo,
            [FromQuery(Name = "pagina")] int pagina) =>
            obtenerApelacionesPorFechaEstadoService.ObtenerApelacionesPorFechaEstadoAsync(HttpContext.TraceIdentifier, fechaInicio, fechaFin, idEstadoReclamo, pagina);

        /// <summary>
        /// Obtiene las apelaciones asociadas a un usuario específico, filtradas por estado y paginadas.
        /// </summary>
        /// <param name="obtenerApelacionesPorUsuarioService">Servicio para obtener las apelaciones por usuario.</param>
        /// <param name="idUsuario">Identificador del usuario externo.</param>
        /// <param name="idEstadoReclamo">Identificador del estado de la apelación.</param>
        /// <param name="pagina">Número de la página a consultar.</param>
        /// <returns>Resultado de la operación de obtención de apelaciones por usuario.</returns>
        [HttpGet("ObtenerApelacionesPorUsuario")]
        public Task<IActionResult> ObtenerApelacionesPorUsuario(
            [FromServices] IObtenerApelacionesPorUsuarioService obtenerApelacionesPorUsuarioService,
            [FromQuery(Name = "id-usuario")] int idUsuario,
            [FromQuery(Name = "id-estado-reclamo")] int idEstadoReclamo,
            [FromQuery(Name = "pagina")] int pagina) =>
            obtenerApelacionesPorUsuarioService.ObtenerApelacionesPorUsuarioAsync(HttpContext.TraceIdentifier, idUsuario, idEstadoReclamo, pagina);
    }
}
