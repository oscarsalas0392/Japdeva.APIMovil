using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Colas.Models;
using Japdeva.APIMovil.Colas.Services.EstadoMensajeService;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;


namespace Japdeva.APIMovil.Colas.Services.ActualizarMensajeFallidoService
{
    /// <summary>
    /// Servicio para actualizar mensajes fallidos en colas del sistema.
    /// </summary>
    public class ActualizarMensajeFallidoService : IActualizarMensajeFallidoService
    {
        private readonly ILogger<ActualizarMensajeFallidoService> _logger;
        private readonly IActualizarRepository _actualizarRepository;
        private readonly IConsultarRepository _consultarRepository;
        private readonly IEstadoMensajeService _estadoMensajeService;
        private const int ID_INVALIDO = 0;
        private const int NUMERO_REINTENTOS = 3;
        private const string NUMERO_MAXIMO_REINTENTOS_ENV_VAR = "NUMERO_MAXIMO_REINTENTOS";
        private const string MENSAJE_ERROR_CAMPO_REQUERIDO = "El {0} del mensaje es requerido.";
        private const string MENSAJE_ERROR_ID_INVALIDO = "El Id: {0} del mensaje no se encuentra.";
        private const string MENSAJE_ERROR_TRACEID_NO_COINCIDE = "El TraceId del mensaje con ID {0} no coincide.";
        private const string MENSAJE_ERROR_ESTADO_FALLIDO = "No se pudo obtener el estado 'Fallido' para actualizar el mensaje.";

        /// <summary>
        /// Inicializa una nueva instancia de la clase ActualizarMensajeFallidoService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="actualizarRepository">Repositorio para actualizar entidades.</param>
        /// <param name="consultarRepository">Repositorio para consultar entidades.</param>
        /// <param name="estadoMensajeService">Servicio para gestionar estados de mensajes.</param>
        public ActualizarMensajeFallidoService(
            ILogger<ActualizarMensajeFallidoService> logger,
            IActualizarRepository actualizarRepository,
            IConsultarRepository consultarRepository,
            IEstadoMensajeService estadoMensajeService)
        {
            this._logger = logger;
            this._actualizarRepository = actualizarRepository;
            this._consultarRepository = consultarRepository;
            this._estadoMensajeService = estadoMensajeService;
        }

        /// <summary>
        /// Actualiza un mensaje como fallido en la cola.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="mensaje">Datos del mensaje fallido a actualizar</param>
        /// <returns>La entidad del mensaje actualizado</returns>
        public async Task<IActionResult> ActualizarMensajeFallidoAsync(string traceId, ActualizarMensajeSolicitudModel mensaje)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                var respuesta = new MensajeColasRespuestaModel();

                if (mensaje.Id <= ID_INVALIDO) throw new ArgumentException(string.Format(MENSAJE_ERROR_CAMPO_REQUERIDO, nameof(mensaje.Id)));
                if (string.IsNullOrEmpty(mensaje.TraceId)) throw new ArgumentException(string.Format(MENSAJE_ERROR_CAMPO_REQUERIDO, nameof(mensaje.TraceId)));

                var mensajeExistente = await this._consultarRepository.ConsultarAsync<MensajeColaEntity>(traceId, m => m.Id == mensaje.Id);
                if (mensajeExistente is null) throw new KeyNotFoundException(string.Format(MENSAJE_ERROR_ID_INVALIDO, mensaje.Id));

                if (mensajeExistente.TraceId != mensaje.TraceId) throw new ArgumentException(string.Format(MENSAJE_ERROR_TRACEID_NO_COINCIDE, mensaje.Id));

                bool esNumero = int.TryParse(Environment.GetEnvironmentVariable(NUMERO_MAXIMO_REINTENTOS_ENV_VAR), out int numeroMaximoReintentos);
                numeroMaximoReintentos = esNumero ? numeroMaximoReintentos : NUMERO_REINTENTOS;

                var estado = mensajeExistente.ContadorReintentos == numeroMaximoReintentos
                            ? this._estadoMensajeService.ObtenerEstadoMensajePorId(traceId, (int)EstadoMensajeModel.Cancelado)
                            : this._estadoMensajeService.ObtenerEstadoMensajePorId(traceId, (int)EstadoMensajeModel.Fallido);

                if (estado is null) throw new Exception(MENSAJE_ERROR_ESTADO_FALLIDO);

                mensajeExistente.EstadoId = estado.Id;
                mensajeExistente.FechaEdicion = DateTime.UtcNow;
                mensajeExistente.TraceId = Guid.NewGuid().ToString();
                mensajeExistente.PrioridadId = (int)PrioridadModel.Baja;
                if (estado.Id == (int)EstadoMensajeModel.Fallido)
                {
                    mensajeExistente.ContadorReintentos++;
                }
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