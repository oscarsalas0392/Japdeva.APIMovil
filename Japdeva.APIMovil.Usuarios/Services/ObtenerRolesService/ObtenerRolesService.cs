using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Usuarios.Models;
using Japdeva.APIMovil.Usuarios.Services.RolCacheService;

namespace Japdeva.APIMovil.Usuarios.Services.ObtenerRolesService
{
    /// <summary>
    /// Servicio para consultar los roles del sistema desde caché.
    /// </summary>
    public class ObtenerRolesService : IObtenerRolesService
    {
        private readonly ILogger<ObtenerRolesService> _logger;
        private readonly IRolCacheService _rolCacheService;
        private const string MENSAJE_ROL_NO_ENCONTRADO = "Rol no encontrado.";

        /// <summary>
        /// Inicializa una nueva instancia de ObtenerRolesService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="rolCacheService">Servicio de caché de roles.</param>
        public ObtenerRolesService(ILogger<ObtenerRolesService> logger, IRolCacheService rolCacheService)
        {
            this._logger = logger;
            this._rolCacheService = rolCacheService;
        }

        /// <summary>
        /// Retorna todos los roles activos almacenados en caché.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <returns>Lista de roles activos.</returns>
        public async Task<IActionResult> ObtenerTodosLosRolesAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                var roles = this._rolCacheService.ObtenerTodos(traceId);
                var lista = new List<RolRespuestaModel>();
                foreach (var rol in roles)
                {
                    var modelo = new RolRespuestaModel();
                    modelo.Id = rol.Id;
                    modelo.Descripcion = rol.Descripcion;
                    modelo.Activo = rol.Activo;
                    lista.Add(modelo);
                }
                return new OkObjectResult(lista);
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

        /// <summary>
        /// Retorna el rol activo con el identificador indicado.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Identificador del rol.</param>
        /// <returns>Resultado con el rol encontrado.</returns>
        public async Task<IActionResult> ObtenerRolPorIdAsync(string traceId, int id)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                var rol = this._rolCacheService.ObtenerPorId(traceId, id);
                if (rol is null) throw new KeyNotFoundException(MENSAJE_ROL_NO_ENCONTRADO);
                var respuesta = new RolRespuestaModel();
                respuesta.Id = rol.Id;
                respuesta.Descripcion = rol.Descripcion;
                respuesta.Activo = rol.Activo;
                return  new OkObjectResult( respuesta );
                
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
