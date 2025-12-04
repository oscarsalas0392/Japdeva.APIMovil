using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Colas.Models;
using Japdeva.APIMovil.Colas.Services.ActualizarMensajeEnProcesoService;
using Japdeva.APIMovil.Colas.Services.ActualizarMensajeExitosoService;
using Japdeva.APIMovil.Colas.Services.ActualizarMensajeFallidoService;
using Japdeva.APIMovil.Colas.Services.EnviarMensajeService;
using Japdeva.APIMovil.Colas.Services.ObtenerMensajePorIdRpcService;
using Japdeva.APIMovil.Colas.Services.ObtenerMensajesPorColaService;


namespace Japdeva.APIMovil.Colas.Controllers
{
    /// <summary>
    /// Controlador para la gestión de mensajes en colas.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MensajeColaController : Controller
    {
        /// <summary>
        /// Envía un mensaje a la cola de mensajería.
        /// </summary>
        /// <param name="enviarMensajeService">Servicio para enviar mensajes.</param>
        /// <param name="mensaje">Modelo con los datos del mensaje a enviar.</param>
        /// <returns>Respuesta del envío del mensaje.</returns>

        [HttpPost("AgregarMensaje")]
        public Task<IActionResult> AgregarMensajeAsync([FromServices] IEnviarMensajeService enviarMensajeService, [FromBody] EnviarMensajeSolicitudModel mensaje) =>
             enviarMensajeService.EnviarMensajeAsync(HttpContext.TraceIdentifier, mensaje);

        /// <summary>
        /// Actualiza un mensaje a estado exitoso.
        /// </summary>
        /// <param name="actualizarMensajeExitosoService">Servicio para actualizar mensajes exitosos.</param>
        /// <param name="mensaje">Modelo con los datos del mensaje a actualizar.</param>
        /// <returns>Respuesta de la actualización del mensaje.</returns>
        [HttpPut("ActualizarMensajeExitoso")]
        public Task<IActionResult> ActualizarMensajeExitosoAsync([FromServices] IActualizarMensajeExitosoService actualizarMensajeExitosoService, [FromBody] ActualizarMensajeSolicitudModel mensaje) =>
            actualizarMensajeExitosoService.ActualizarMensajeExitosoAsync(HttpContext.TraceIdentifier, mensaje);

        /// <summary>
        /// Actualiza un mensaje a estado en proceso.
        /// </summary>
        /// <param name="actualizarMensajeEnProcesoService">Servicio para actualizar mensajes en proceso.</param>
        /// <param name="mensaje">Modelo con los datos del mensaje a actualizar.</param>
        /// <returns>Respuesta de la actualización del mensaje.</returns>
        [HttpPut("ActualizarMensajeEnProceso")]
        public Task<IActionResult> ActualizarMensajeEnProcesoAsync([FromServices] IActualizarMensajeEnProcesoService actualizarMensajeEnProcesoService, [FromBody] ActualizarMensajeSolicitudModel mensaje) =>
            actualizarMensajeEnProcesoService.ActualizarMensajeEnProcesoAsync(HttpContext.TraceIdentifier, mensaje);


        /// <summary>
        /// Actualiza un mensaje a estado fallido.
        /// </summary>
        /// <param name="actualizarMensajeFallidoService">Servicio para actualizar mensajes fallidos.</param>
        /// <param name="mensaje">Modelo con los datos del mensaje a actualizar.</param>
        /// <returns>Respuesta de la actualización del mensaje.</returns>
        [HttpPut("ActualizarMensajeFallido")]
        public Task<IActionResult> ActualizarMensajeFallidoAsync([FromServices] IActualizarMensajeFallidoService actualizarMensajeFallidoService, [FromBody] ActualizarMensajeSolicitudModel mensaje) =>
            actualizarMensajeFallidoService.ActualizarMensajeFallidoAsync(HttpContext.TraceIdentifier, mensaje);

        /// <summary>
        /// Obtiene los mensajes pendientes de una cola específica.
        /// </summary>
        /// <param name="obtenerMensajesPorColaService">Servicio para obtener mensajes por cola.</param>
        /// <param name="nombreCola">Nombre de la cola de la cual obtener los mensajes pendientes.</param>
        /// <returns>Respuesta con los mensajes pendientes de la cola.</returns>
    [HttpGet("ObtenerMensajesPendientes")]
    public Task<IActionResult> ObtenerMensajesPendientesAsync([FromServices] IObtenerMensajesPorColaService obtenerMensajesPorColaService, [FromQuery(Name = "nombre-cola")] string nombreCola) =>
        obtenerMensajesPorColaService.ObtenerMensajesPorColaAsync(HttpContext.TraceIdentifier, nombreCola);

    /// <summary>
    /// Obtiene un mensaje específico por su ID RPC.
    /// </summary>
    /// <param name="obtenerMensajesPorIdRpcService">Servicio para obtener mensajes por ID RPC.</param>
    /// <param name="idRpc">ID RPC del mensaje a buscar.</param>
    /// <param name="nombreCola">Nombre de la cola donde buscar el mensaje.</param>
    /// <returns>Respuesta con el mensaje encontrado por ID RPC.</returns>
    [HttpGet("ObtenerMensajesPendientesPorIdRpc")]
    public Task<IActionResult> ObtenerMensajesPendientesPorIdRpcAsync([FromServices] IObtenerMensajePorIdRpcService obtenerMensajesPorIdRpcService, [FromQuery(Name = "id-rpc")] string idRpc, [FromQuery(Name = "nombre-cola")] string nombreCola) =>
        obtenerMensajesPorIdRpcService.ObtenerMensajeRpcAsync(HttpContext.TraceIdentifier, idRpc, nombreCola);

    }
}