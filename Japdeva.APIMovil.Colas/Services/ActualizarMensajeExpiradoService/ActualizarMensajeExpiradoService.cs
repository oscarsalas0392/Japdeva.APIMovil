using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Colas.Models;
using Japdeva.APIMovil.Colas.Services.EstadoMensajeService;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;

namespace Japdeva.APIMovil.Colas.Services.ActualizarMensajeExpiradoService
{
    /// <summary>
    /// Servicio para actualizar mensajes expirados en colas del sistema.
    /// </summary>
    public class ActualizarMensajeExpiradoService : IActualizarMensajeExpiradoService
    {
        private readonly ILogger<ActualizarMensajeExpiradoService> _logger;
        private readonly IEstadoMensajeService _estadoMensajeService;
        private readonly IServiceProvider _serviceProvider;

        private const string MENSAJE_ERROR_CAMPO_REQUERIDO = "El {0} del mensaje es requerido.";
        private const string MENSAJE_ERROR_ID_INVALIDO = "El Id: {0} del mensaje no se encuentra."; 
        private const string MENSAJE_ERROR_ESTADO_EXPIRADO = "No se pudo obtener el estado 'Expirado' para actualizar el mensaje.";
        private const string MENSAJE_EXPIRADO = "Mensaje expirado - Tiempo límite excedido";
        private const string MENSAJE_EXPIRADO_EN = " en ";
        private const string MENSAJE_EXPIRADO_UTC = " UTC";
        private const string FORMATO_FECHA_MENSAJE = "yyyy-MM-dd HH:mm:ss";
        private const int MENSAJE_ID_MINIMO = 0;
        private const bool TRACEID_DIFERENTE_INICIAL = false;

        /// <summary>
        /// Inicializa una nueva instancia de la clase ActualizarMensajeExpiradoService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="estadoMensajeService">Servicio para gestionar estados de mensajes.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        public ActualizarMensajeExpiradoService(
            ILogger<ActualizarMensajeExpiradoService> logger,
            IEstadoMensajeService estadoMensajeService, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._estadoMensajeService = estadoMensajeService;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Actualiza un mensaje como expirado en la cola.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="idMensaje">Identificador del mensaje a actualizar</param>
        /// <param name="traceIdMensaje">Identificador de trazabilidad del mensaje</param>
        /// <returns>La entidad del mensaje actualizado</returns>
        public async Task<IActionResult> ActualizarMensajeExpiradoAsync(string traceId, long idMensaje, string traceIdMensaje)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var actualizarRepository = scope.ServiceProvider.GetRequiredService<IActualizarRepository>(); 
                var respuesta = new MensajeColasRespuestaModel();
                bool traceidDiferente = TRACEID_DIFERENTE_INICIAL;

                if (idMensaje <= MENSAJE_ID_MINIMO) throw new ArgumentException(string.Format(MENSAJE_ERROR_CAMPO_REQUERIDO, nameof(idMensaje)));
                if (string.IsNullOrEmpty(traceIdMensaje)) throw new ArgumentException(string.Format(MENSAJE_ERROR_CAMPO_REQUERIDO, nameof(traceIdMensaje)));

                var mensajeExistente = await consultarRepository.ConsultarAsync<MensajeColaEntity>(traceId, m => m.Id == idMensaje);
                if (mensajeExistente is null) throw new KeyNotFoundException(string.Format(MENSAJE_ERROR_ID_INVALIDO, idMensaje));

                if (mensajeExistente.TraceId != traceIdMensaje) traceidDiferente = true;

                if (!traceidDiferente)
                {
                    var estado = this._estadoMensajeService.ObtenerEstadoMensajePorId(traceId, (int)EstadoMensajeModel.Expirado);
                    if (estado is null) throw new KeyNotFoundException(MENSAJE_ERROR_ESTADO_EXPIRADO);

                    mensajeExistente.EstadoId = (int)EstadoMensajeModel.Expirado;
                    mensajeExistente.FechaEdicion = DateTime.UtcNow;
                    mensajeExistente.TraceId = Guid.NewGuid().ToString();
                    string fechaActual = DateTime.UtcNow.ToString(FORMATO_FECHA_MENSAJE);
                    mensajeExistente.MensajeError = $"{MENSAJE_EXPIRADO}{MENSAJE_EXPIRADO_EN}{fechaActual}{MENSAJE_EXPIRADO_UTC}";

                    await actualizarRepository.ActualizarAsync<MensajeColaEntity>(traceId, mensajeExistente);
                }
                respuesta.Id = mensajeExistente.Id;
                respuesta.Cola = mensajeExistente.ColaId;
                respuesta.Mensaje = mensajeExistente.ContenidoMensaje;
                respuesta.TraceId = mensajeExistente.TraceId;
                respuesta.Estado = mensajeExistente.EstadoId;
                respuesta.MetaDatos = mensajeExistente.Metadatos;
                respuesta.TraceIdDiferente = traceidDiferente;

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