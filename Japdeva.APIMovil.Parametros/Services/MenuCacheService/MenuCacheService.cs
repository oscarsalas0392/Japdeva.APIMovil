using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Parametros.Entities;

namespace Japdeva.APIMovil.Parametros.Services.MenuCacheService
{
    /// <summary>
    /// Servicio para gestionar la caché de menús en memoria, obteniendo los datos desde el repositorio
    /// y permitiendo su acceso eficiente.
    /// </summary>
    public class MenuCacheService : IMenuCacheService
    {
        private readonly ILogger<MenuCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<MenuEntity> _menuCache = new List<MenuEntity>();

        private const int PAGINA_INICIAL = 1;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="MenuCacheService"/>.
        /// </summary>
        /// <param name="logger">El registrador de eventos para la clase <see cref="MenuCacheService"/>.</param>
        /// <param name="serviceProvider">El proveedor de servicios para la resolución de dependencias.</param>
        public MenuCacheService(ILogger<MenuCacheService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Llena la caché de menús obteniendo los datos desde el repositorio y almacenándolos en memoria.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el registro de logs.</param>
        public async Task LlenarCacheMenuAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                List<MenuEntity> listaMenuCache = new List<MenuEntity>();
                var scope = this._serviceProvider.CreateScope();
                var consultarListaEntity = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                int paginaActual = PAGINA_INICIAL;
                int totalPaginas = PAGINA_INICIAL;

                do
                {
                    var respuestaLista = await consultarListaEntity.ConsultarListaAsync<MenuEntity>(traceId, paginaActual);
                    totalPaginas = respuestaLista.CantidadPaginas;
                    paginaActual++;
                    listaMenuCache.AddRange(respuestaLista.Lista);
                }
                while (paginaActual <= totalPaginas);

                lock (this._menuCache)
                {
                    this._menuCache.Clear();
                    this._menuCache.AddRange(listaMenuCache);
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
        /// Obtiene una lista de menús desde la caché que coinciden con los identificadores proporcionados y que están activos.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el registro de logs.</param>
        /// <param name="listaIds">Lista de identificadores de menú a buscar.</param>
        /// <returns>Lista de objetos <see cref="MenuEntity"/> que cumplen con los criterios.</returns>
        public List<MenuEntity> ObtenerMenuPorId(string traceId, List<int> listaIds)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();

            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                return this._menuCache.Where(m => listaIds.Contains(m.Id) && m.Activo).ToList();
                
            }
            catch (Exception ex)
            {
                this._logger.Error(string.Empty, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(traceId, nombreMetodo);
            }
        }
    }
}
