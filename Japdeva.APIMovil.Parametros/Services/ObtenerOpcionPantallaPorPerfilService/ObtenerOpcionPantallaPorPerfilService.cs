using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Parametros.Models;
using Japdeva.APIMovil.Parametros.Services.OpcionPantallaCacheService;
using Japdeva.APIMovil.Parametros.Services.OpcionPantallaPerfilCacheService;

namespace Japdeva.APIMovil.Parametros.Services.ObtenerOpcionPantallaPorPerfilService
{
    /// <summary>
    /// Servicio para obtener las opciones de pantalla asociadas a un perfil específico desde la caché.
    /// Combina información de OpcionPantallaPerfil y OpcionPantalla para devolver las opciones activas de un perfil.
    /// </summary>
    public class ObtenerOpcionPantallaPorPerfilService : IObtenerOpcionPantallaPorPerfilService
    {
        private readonly ILogger<ObtenerOpcionPantallaPorPerfilService> _logger;
        private readonly IOpcionPantallaPerfilCacheService _opcionPantallaPerfilCacheService;
        private readonly IOpcionPantallaCacheService _opcionPantallaCacheService;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerOpcionPantallaPorPerfilService"/>.
        /// </summary>
        /// <param name="logger">El registrador de eventos para la clase <see cref="ObtenerOpcionPantallaPorPerfilService"/>.</param>
        /// <param name="opcionPantallaPerfilCacheService">Servicio de caché de opción de pantalla por perfil.</param>
        /// <param name="opcionPantallaCacheService">Servicio de caché de opciones de pantalla.</param>
        public ObtenerOpcionPantallaPorPerfilService(
            ILogger<ObtenerOpcionPantallaPorPerfilService> logger,
            IOpcionPantallaPerfilCacheService opcionPantallaPerfilCacheService,
            IOpcionPantallaCacheService opcionPantallaCacheService)
        {
            this._logger = logger;
            this._opcionPantallaPerfilCacheService = opcionPantallaPerfilCacheService;
            this._opcionPantallaCacheService = opcionPantallaCacheService;
        }

        /// <summary>
        /// Obtiene las opciones de pantalla activas asociadas a un perfil específico desde la caché.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idPerfil">Identificador del perfil para filtrar las opciones.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        public async Task<IActionResult> ObtenerOpcionPantallaPorPerfilAsync(string traceId, int idPerfil)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                var opcionPantallaPerfilEntities = this._opcionPantallaPerfilCacheService.ObtenerOpcionPantallaPerfilPorIdPerfil(traceId, idPerfil);
                var idsOpcionPantalla = opcionPantallaPerfilEntities.Select(op => op.IdOpcionPantalla).ToList();

                List<OpcionPantallaRespuestaModel> opcionesRespuesta = new List<OpcionPantallaRespuestaModel>();

                if (idsOpcionPantalla.Any())
                {
                    var opcionPantallaEntities = this._opcionPantallaCacheService.ObtenerOpcionPantallaPorId(traceId, idsOpcionPantalla);

                    foreach (var opcion in opcionPantallaEntities)
                    {
                        OpcionPantallaRespuestaModel opcionRespuesta = new OpcionPantallaRespuestaModel
                        {
                            Id = opcion.Id,
                            Nombre = opcion.Nombre,
                        };

                        opcionesRespuesta.Add(opcionRespuesta);
                    }
                }

                return await Task.FromResult(new OkObjectResult(opcionesRespuesta));
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
