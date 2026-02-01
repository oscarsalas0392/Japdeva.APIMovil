using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Parametros.Models;
using Japdeva.APIMovil.Parametros.Services.MensajeCacheService;
using Japdeva.APIMovil.Parametros.Services.TipoMensajeCacheService;

namespace Japdeva.APIMovil.Parametros.Services.ObtenerMensajePorPantallaService
{
    /// <summary>
    /// Servicio para obtener mensajes desde la caché basándose en el identificador de pantalla.
    /// Convierte las entidades de caché en modelos de respuesta para su consumo.
    /// </summary>
    public class ObtenerMensajePorPantallaService : IObtenerMensajePorPantallaService
    {
        private readonly ILogger<ObtenerMensajePorPantallaService> _logger;
        private readonly IMensajeCacheService _mensajeCacheService;
        private readonly ITipoMensajeCacheService _tipoMensajeCacheService;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerMensajePorPantallaService"/>.
        /// </summary>
        /// <param name="logger">El registrador de eventos para la clase <see cref="ObtenerMensajePorPantallaService"/>.</param>
        /// <param name="mensajeCacheService">Servicio de caché de mensajes.</param>
        /// <param name="tipoMensajeCacheService">Servicio de caché de tipos de mensaje.</param>
        public ObtenerMensajePorPantallaService(
            ILogger<ObtenerMensajePorPantallaService> logger,
            IMensajeCacheService mensajeCacheService,
            ITipoMensajeCacheService tipoMensajeCacheService)
        {
            this._logger = logger;
            this._mensajeCacheService = mensajeCacheService;
            this._tipoMensajeCacheService = tipoMensajeCacheService;
        }

        /// <summary>
        /// Obtiene los mensajes activos asociados a una pantalla específica desde la caché.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idPantalla">Identificador de la pantalla para filtrar los mensajes.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        public Task<IActionResult> ObtenerMensajesPorPantallaAsync(string traceId, int idPantalla)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                var mensajesEntity = this._mensajeCacheService.ObtenerMensajes(traceId, idPantalla);

                List<MensajeRespuestaModel> mensajesRespuesta = new List<MensajeRespuestaModel>();

                foreach (var mensaje in mensajesEntity)
                {
                    var tipoMensaje = this._tipoMensajeCacheService.ObtenerTipoMensajePorId(traceId, mensaje.IdTipoMensaje);

                    MensajeRespuestaModel mensajeRespuesta = new MensajeRespuestaModel
                    {
                        Id = mensaje.Id,
                        Descripcion = mensaje.Descripcion,
                        IdTipoMensaje = mensaje.IdTipoMensaje,
                        DescripcionTipoMensaje = tipoMensaje?.Descripcion ?? string.Empty,
                        IdPantalla = mensaje.IdPantalla,
                        IdUsuarioInterno = mensaje.IdUsuarioInterno
                    };

                    mensajesRespuesta.Add(mensajeRespuesta);
                }

                return Task.FromResult<IActionResult>(new OkObjectResult(mensajesRespuesta));
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
