using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Parametros.Models;
using Japdeva.APIMovil.Parametros.Services.MenuCacheService;
using Japdeva.APIMovil.Parametros.Services.MenuPerfilCacheService;

namespace Japdeva.APIMovil.Parametros.Services.ObtenerMenuPorPerfilService
{
    /// <summary>
    /// Servicio para obtener los menús asociados a un perfil específico desde la caché.
    /// Combina información de MenuPerfil y Menu para devolver los menús activos de un perfil.
    /// </summary>
    public class ObtenerMenuPorPerfilService : IObtenerMenuPorPerfilService
    {
        private readonly ILogger<ObtenerMenuPorPerfilService> _logger;
        private readonly IMenuPerfilCacheService _menuPerfilCacheService;
        private readonly IMenuCacheService _menuCacheService;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerMenuPorPerfilService"/>.
        /// </summary>
        /// <param name="logger">El registrador de eventos para la clase <see cref="ObtenerMenuPorPerfilService"/>.</param>
        /// <param name="menuPerfilCacheService">Servicio de caché de menú-perfil.</param>
        /// <param name="menuCacheService">Servicio de caché de menús.</param>
        public ObtenerMenuPorPerfilService(
            ILogger<ObtenerMenuPorPerfilService> logger,
            IMenuPerfilCacheService menuPerfilCacheService,
            IMenuCacheService menuCacheService)
        {
            this._logger = logger;
            this._menuPerfilCacheService = menuPerfilCacheService;
            this._menuCacheService = menuCacheService;
        }

        /// <summary>
        /// Obtiene la lista de menús activos asociados a un perfil específico desde la caché.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idPerfil">Identificador del perfil para filtrar los menús.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        public Task<IActionResult> ObtenerMenusPorPerfilAsync(string traceId, int idPerfil)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                // Obtener las relaciones menu-perfil para el perfil especificado
                var menuPerfilEntities = this._menuPerfilCacheService.ObtenerMenuPerfilPorIdPerfil(traceId, idPerfil);

                // Extraer los IDs de menú
                var idsMenu = menuPerfilEntities.Select(mp => mp.IdMenu).ToList();

                List<MenuRespuestaModel> menusRespuesta = new List<MenuRespuestaModel>();

                // Si hay menús asociados, obtenerlos
                if (idsMenu.Any())
                {
                    // Obtener los menús usando los IDs
                    var menuEntities = this._menuCacheService.ObtenerMenuPorId(traceId, idsMenu);

                    // Mapear a modelo de respuesta
                    foreach (var menu in menuEntities)
                    {
                        MenuRespuestaModel menuRespuesta = new MenuRespuestaModel
                        {
                            Id = menu.Id,
                            Descripcion = menu.Descripcion,
                            Mostrar = menu.Mostrar,
                            IdUsuarioInterno = menu.IdUsuarioInterno
                        };

                        menusRespuesta.Add(menuRespuesta);
                    }
                }

                return Task.FromResult<IActionResult>(new OkObjectResult(menusRespuesta));
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
