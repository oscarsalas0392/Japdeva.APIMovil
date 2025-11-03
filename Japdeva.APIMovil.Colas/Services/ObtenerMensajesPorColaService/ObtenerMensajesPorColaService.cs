using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Colas.Services.MensajeColaService;
using Japdeva.APIMovil.Common.Extensions;

namespace Japdeva.APIMovil.Colas.Services.ObtenerMensajesPorColaService
{
    /// <summary>
    /// Servicio para obtener mensajes de una cola específica.
    /// </summary>
    public class ObtenerMensajesPorColaService : IObtenerMensajesPorColaService
    {
        private readonly ILogger<ObtenerMensajesPorColaService> _logger;
        private readonly IMensajeColaService _mensajeColaService;
        private const string MENSAJE_NOMBRE_COLA_REQUERIDO = "El nombre de cola es requerido.";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerMensajesPorColaService"/>.
        /// </summary>
        /// <param name="logger">El logger para registrar eventos y errores.</param>
        /// <param name="mensajeColaService">El servicio para manejar mensajes de cola.</param>
        public ObtenerMensajesPorColaService(ILogger<ObtenerMensajesPorColaService> logger, IMensajeColaService mensajeColaService)
        {
            this._logger = logger;
            this._mensajeColaService = mensajeColaService;
        }

        /// <summary>
        /// Obtiene los mensajes de una cola específica.
        /// </summary>
        /// <param name="traceId">El identificador de trazabilidad.</param>
        /// <param name="nombreCola">El nombre de la cola.</param>
        /// <returns>Una tarea que representa la operación asíncrona que contiene el resultado de la acción.</returns>
        public async Task<IActionResult> ObtenerMensajesPorColaAsync(string traceId, string nombreCola)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (string.IsNullOrWhiteSpace(nombreCola))
                {
                    throw new ArgumentException(MENSAJE_NOMBRE_COLA_REQUERIDO, nameof(nombreCola));
                }
                var mensajes = await this._mensajeColaService.ObtenerMensajesNombreColaAsync(traceId, nombreCola);
                return new ObjectResult(mensajes);
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