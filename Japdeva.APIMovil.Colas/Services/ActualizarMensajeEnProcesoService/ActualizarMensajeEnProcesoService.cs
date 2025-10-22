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
        private readonly IActualizarRepository _actualizarRepository;
        private readonly IConsultarRepository _consultarRepository;
        private readonly IEstadoMensajeService _estadoMensajeService;

        private const string MENSAJE_ERROR_ID_INVALIDO = "El identificador del mensaje no es válido.";
        private const string MENSAJE_ERROR_TRACEID_INVALIDO = "El TraceId del mensaje no es válido.";
        private const string MENSAJE_ERROR_ESTADO_ENPROCESO = "No se pudo obtener el estado 'EnProceso' para actualizar el mensaje.";
        private const bool TRACE_ID_VALIDO_INICIAL = true;
        private const int MENSAJE_ID_MINIMO = 0;

        /// <summary>
        /// Inicializa una nueva instancia de la clase ActualizarMensajeEnProcesoService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="actualizarRepository">Repositorio para actualizar entidades.</param>
        /// <param name="consultarRepository">Repositorio para consultar entidades.</param>
        /// <param name="estadoMensajeService">Servicio para gestionar estados de mensajes.</param>
        public ActualizarMensajeEnProcesoService(
            ILogger<ActualizarMensajeEnProcesoService> logger,
            IActualizarRepository actualizarRepository,
            IEstadoMensajeService estadoMensajeService,
            IConsultarRepository consultarRepository)
        {
            this._logger = logger;
            this._actualizarRepository = actualizarRepository;
            this._consultarRepository = consultarRepository;
            this._estadoMensajeService = estadoMensajeService;
        }

        /// <summary>
        /// Actualiza un mensaje como en proceso en la cola.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="mensaje">Datos del mensaje en proceso a actualizar</param>
        /// <returns>La entidad del mensaje actualizado</returns>
        public async Task<IActionResult> ActualizarMensajeEnProcesoAsync(string traceId, EnviarMensajeSolicitudModel mensaje)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                bool traceIdValido = TRACE_ID_VALIDO_INICIAL;
                this._logger.Inicio(traceId, nombreMetodo);
                var respuesta = new MensajeColasRespuestaModel();

                if (mensaje.Id <= MENSAJE_ID_MINIMO) throw new ArgumentException(MENSAJE_ERROR_ID_INVALIDO);
                if (string.IsNullOrEmpty(mensaje.TraceId)) throw new ArgumentException(MENSAJE_ERROR_TRACEID_INVALIDO);

                var mensajeExistente = await this._consultarRepository.ConsultarAsync<MensajeColaEntity>(traceId, m => m.Id == mensaje.Id);
                if (mensajeExistente is null) throw new KeyNotFoundException(MENSAJE_ERROR_ID_INVALIDO);

                if (mensajeExistente.TraceId != mensaje.TraceId) traceIdValido = false;
                
                if(traceIdValido) 
                {
                    var estado = this._estadoMensajeService.ObtenerEstadoMensajePorId(traceId, (int)EstadoMensajeModel.EnProceso);
                    if (estado is null) throw new Exception(MENSAJE_ERROR_ESTADO_ENPROCESO);
                    mensajeExistente.EstadoId = estado.Id;
                    mensajeExistente.FechaEdicion = DateTime.UtcNow;
                    mensajeExistente.TraceId = Guid.NewGuid().ToString();
                    await this._actualizarRepository.ActualizarAsync<MensajeColaEntity>(traceId, mensajeExistente);
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