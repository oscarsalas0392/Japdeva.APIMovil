using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Colas.Models;
using Japdeva.APIMovil.Colas.Services.ColaService;
using Japdeva.APIMovil.Colas.Services.EstadoMensajeService;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;

namespace Japdeva.APIMovil.Colas.Services.ActualizarMensajeExitosoService
{
    /// <summary>
    /// Servicio para actualizar mensajes en colas del sistema.
    /// </summary>
    public class ActualizarMensajeExitosoService : IActualizarMensajeExitosoService
    {
        private readonly ILogger<ActualizarMensajeExitosoService> _logger;
        private readonly IActualizarRepository _actualizarRepository;
        private readonly IColaService _colaService;
        private readonly IConsultarRepository _consultarRepository;
        private readonly IEstadoMensajeService _estadoMensajeService;

        private const string MENSAJE_ERROR_CAMPO_REQUERIDO = "El {0} del mensaje es requerido.";
        private const string MENSAJE_ERROR_ID_INVALIDO = "El Id: {0} del mensaje no se encuentra.";
        private const string MENSAJE_ERROR_TRACEID_NO_COINCIDE = "El TraceId del mensaje con ID {0} no coincide.";
        private const string MENSAJE_ERROR_ESTADO_PROCESADO = "No se pudo obtener el estado 'Procesado' para actualizar el mensaje.";
        private const int MENSAJE_ID_MINIMO = 0;

        /// <summary>
        /// Inicializa una nueva instancia de la clase ActualizarMensajeService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="actualizarRepository">Repositorio para actualizar entidades.</param>
        /// <param name="consultarRepository">Repositorio para consultar entidades.</param>
        /// <param name="estadoMensajeService">Servicio para gestionar estados de mensajes.</param>
        /// <param name="colaService">Servicio para gestionar colas del sistema.</param>
        public ActualizarMensajeExitosoService(
            ILogger<ActualizarMensajeExitosoService> logger,
            IActualizarRepository actualizarRepository,
            IConsultarRepository consultarRepository,
            IEstadoMensajeService estadoMensajeService, IColaService colaService)
        {
            this._logger = logger;
            this._actualizarRepository = actualizarRepository;
            this._consultarRepository = consultarRepository;
            this._estadoMensajeService = estadoMensajeService;
            this._colaService = colaService;

        }

        /// <summary>
        /// Actualiza un mensaje existente en la cola.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="mensaje">Datos del mensaje a actualizar</param>
        /// <returns>La entidad del mensaje actualizado</returns>
        public async Task<IActionResult> ActualizarMensajeExitosoAsync(string traceId, ActualizarMensajeSolicitudModel mensaje)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                var respuesta = new MensajeColasRespuestaModel();
                if (mensaje.Id <= MENSAJE_ID_MINIMO) throw new ArgumentException(string.Format(MENSAJE_ERROR_CAMPO_REQUERIDO, nameof(mensaje.Id)));
                if (string.IsNullOrEmpty(mensaje.TraceId)) throw new ArgumentException(string.Format(MENSAJE_ERROR_CAMPO_REQUERIDO, nameof(mensaje.TraceId)));

                var mensajeExistente = await this._consultarRepository.ConsultarAsync<MensajeColaEntity>(traceId, m => m.Id == mensaje.Id);
                if (mensajeExistente is null) throw new KeyNotFoundException(string.Format(MENSAJE_ERROR_ID_INVALIDO, mensaje.Id));

                if (mensajeExistente.TraceId != mensaje.TraceId) throw new ArgumentException(string.Format(MENSAJE_ERROR_TRACEID_NO_COINCIDE, mensaje.Id));

                var estado = this._estadoMensajeService.ObtenerEstadoMensajePorId(traceId, (int)EstadoMensajeModel.Procesado);
                if (estado is null) throw new KeyNotFoundException(MENSAJE_ERROR_ESTADO_PROCESADO);

                mensajeExistente.EstadoId = estado.Id;
                mensajeExistente.TraceId = Guid.NewGuid().ToString();

                await this._actualizarRepository.ActualizarAsync<MensajeColaEntity>(traceId, mensajeExistente);

                respuesta.Id = mensajeExistente.Id;
                respuesta.Cola = mensajeExistente.ColaId;
                respuesta.Mensaje = mensajeExistente.ContenidoMensaje;
                respuesta.TraceId = mensajeExistente.TraceId;
                respuesta.Estado = mensajeExistente.EstadoId;
                respuesta.MetaDatos = mensajeExistente.Metadatos;

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