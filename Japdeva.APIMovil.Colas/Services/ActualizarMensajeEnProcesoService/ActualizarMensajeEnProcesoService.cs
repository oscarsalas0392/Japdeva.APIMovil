using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Colas.Models;
using Japdeva.APIMovil.Colas.Services.EstadoMensajeService;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;


namespace Japdeva.APIMovil.Colas.Services.ActualizarMensajeEnProcesoService
{
    /// <summary>
    /// Servicio para actualizar mensajes en proceso en colas del sistema.
    /// </summary>
    public class ActualizarMensajeEnProcesoService : IActualizarMensajeEnProcesoService
    {
        private readonly ILogger<ActualizarMensajeEnProcesoService> _logger;
        private readonly IEstadoMensajeService _estadoMensajeService;
        private readonly IServiceProvider _serviceProvider;

        private const string MENSAJE_ERROR_CAMPO_REQUERIDO = "El {0} del mensaje es requerido.";
        private const string MENSAJE_ERROR_ID_INVALIDO = "El Id: {0} del mensaje no se encuentra.";
        private const string MENSAJE_ERROR_ESTADO_ENPROCESO = "No se pudo obtener el estado 'EnProceso' para actualizar el mensaje.";
        private const bool TRACE_ID_VALIDO_INICIAL = true;
        private const int MENSAJE_ID_MINIMO = 0;

        /// <summary>
        /// Inicializa una nueva instancia de la clase ActualizarMensajeEnProcesoService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="estadoMensajeService">Servicio para gestionar estados de mensajes.</param>
        /// <param name="serviceProvider">Proveedor de servicios para inyección de dependencias.</param>
        public ActualizarMensajeEnProcesoService(
            ILogger<ActualizarMensajeEnProcesoService> logger,
            IEstadoMensajeService estadoMensajeService, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._estadoMensajeService = estadoMensajeService;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Actualiza un mensaje como en proceso en la cola.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="mensaje">Datos del mensaje en proceso a actualizar</param>
        /// <returns>La entidad del mensaje actualizado</returns>
        public async Task<IActionResult> ActualizarMensajeEnProcesoAsync(string traceId, ActualizarMensajeSolicitudModel mensaje)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var actualizarRepository = scope.ServiceProvider.GetRequiredService<IActualizarRepository>();
                bool traceIdValido = TRACE_ID_VALIDO_INICIAL;
                this._logger.Inicio(traceId, nombreMetodo);
                var respuesta = new MensajeColasRespuestaModel();

                if (mensaje.Id <= MENSAJE_ID_MINIMO) throw new ArgumentException(string.Format(MENSAJE_ERROR_CAMPO_REQUERIDO, nameof(mensaje.Id)));
                if (string.IsNullOrEmpty(mensaje.TraceId)) throw new ArgumentException(string.Format(MENSAJE_ERROR_CAMPO_REQUERIDO, nameof(mensaje.TraceId)));

                var mensajeExistente = await consultarRepository.ConsultarAsync<MensajeColaEntity>(traceId, m => m.Id == mensaje.Id);
                if (mensajeExistente is null) throw new KeyNotFoundException(string.Format(MENSAJE_ERROR_ID_INVALIDO, mensaje.Id));

                if (mensajeExistente.TraceId != mensaje.TraceId) traceIdValido = false;

                if (traceIdValido)
                {
                    var estado = this._estadoMensajeService.ObtenerEstadoMensajePorId(traceId, (int)EstadoMensajeModel.EnProceso);
                    if (estado is null) throw new KeyNotFoundException(MENSAJE_ERROR_ESTADO_ENPROCESO);
                    mensajeExistente.EstadoId = estado.Id;
                    mensajeExistente.FechaEdicion = DateTime.UtcNow;
                    mensajeExistente.TraceId = Guid.NewGuid().ToString();
                    await actualizarRepository.ActualizarAsync<MensajeColaEntity>(traceId, mensajeExistente);
                }
                else
                {
                    respuesta.TraceIdDiferente = !traceIdValido;
                }

                respuesta.Id = mensajeExistente.Id;
                respuesta.Cola = mensajeExistente.ColaId;
                respuesta.TraceId = mensajeExistente.TraceId;
                respuesta.Estado = mensajeExistente.EstadoId;
                respuesta.Mensaje = mensajeExistente.ContenidoMensaje;

                return new OkObjectResult(respuesta);
            }
            catch (ArgumentException ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw;
            }
            catch (Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(traceId, nombreMetodo);
            }
        }
    }
}