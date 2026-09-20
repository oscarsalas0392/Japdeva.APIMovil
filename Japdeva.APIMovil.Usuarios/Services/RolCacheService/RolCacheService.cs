using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Usuarios.Entities;

namespace Japdeva.APIMovil.Usuarios.Services.RolCacheService
{
    /// <summary>
    /// Servicio de caché para los roles del sistema.
    /// Carga y mantiene en memoria todos los roles activos para consulta sin acceso a base de datos.
    /// </summary>
    public class RolCacheService : IRolCacheService
    {
        private readonly ILogger<RolCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<RolEntity> _rolesCache = new List<RolEntity>();

        private const int PAGINA_INICIAL = 1;

        /// <summary>
        /// Inicializa el servicio de caché de roles.
        /// </summary>
        /// <param name="logger">Logger para registro de operaciones.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias.</param>
        public RolCacheService(ILogger<RolCacheService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Carga o recarga todos los roles activos desde la base de datos.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        public async Task LlenarCacheRolesAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                List<RolEntity> lista = new List<RolEntity>();
                using var scope = this._serviceProvider.CreateScope();
                var repositorio = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                int paginaActual = PAGINA_INICIAL;
                int totalPaginas = PAGINA_INICIAL;
                do
                {
                    var respuesta = await repositorio.ConsultarListaAsync<RolEntity>(traceId, paginaActual);
                    totalPaginas = respuesta.CantidadPaginas;
                    paginaActual++;
                    lista.AddRange(respuesta.Lista);
                }
                while (paginaActual <= totalPaginas);
                lock (this._rolesCache)
                {
                    this._rolesCache.Clear();
                    this._rolesCache.AddRange(lista);
                }
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
        /// Retorna todos los roles activos almacenados en caché.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <returns>Lista de roles activos.</returns>
        public List<RolEntity> ObtenerTodos(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                return this._rolesCache.Where(r => r.Activo).ToList();
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
        /// Retorna el rol activo con el identificador indicado, o null si no existe.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Identificador del rol.</param>
        /// <returns>El rol encontrado o null.</returns>
        public RolEntity? ObtenerPorId(string traceId, int id)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                return this._rolesCache.FirstOrDefault(r => r.Id == id && r.Activo);
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
