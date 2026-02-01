using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Parametros.Entities;


namespace Japdeva.APIMovil.Parametros.Services.MenuPerfilCacheService
{
    /// <summary>
    /// Servicio para gestionar la caché de menús por perfil en memoria, obteniendo los datos desde el repositorio
    /// y permitiendo su acceso eficiente.
    /// </summary>
    public class MenuPerfilCacheService: IMenuPerfilCacheService
    {
        private readonly ILogger<MenuPerfilCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<MenuPerfilEntity> _menuPerfilCache = new List<MenuPerfilEntity>();

        private const int PAGINA_INICIAL = 1;


        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="MenuPerfilCacheService"/>.
        /// </summary>
        /// <param name="logger">Instancia del registrador de logs.</param>
        /// <param name="serviceProvider">Proveedor de servicios para la resolución de dependencias.</param>
        public MenuPerfilCacheService(ILogger<MenuPerfilCacheService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }


        /// <summary>
        /// Llena la caché de menús por perfil obteniendo los datos desde el repositorio y almacenándolos en memoria.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        public async Task LlenarCacheMenuPerfilAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                List<MenuPerfilEntity> listaMenuPerfilCache = new List<MenuPerfilEntity>();
                var scope = this._serviceProvider.CreateScope();
                var consultarListaEntity = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                int paginaActual = PAGINA_INICIAL;
                int totalPaginas = PAGINA_INICIAL;

                do
                {
                    var respuestaLista = await consultarListaEntity.ConsultarListaAsync<MenuPerfilEntity>(traceId, paginaActual);
                    totalPaginas = respuestaLista.CantidadPaginas;
                    paginaActual++;
                    listaMenuPerfilCache.AddRange(respuestaLista.Lista);
                }
                while (paginaActual <= totalPaginas);

                lock (this._menuPerfilCache)
                {
                    this._menuPerfilCache.Clear();
                    this._menuPerfilCache.AddRange(listaMenuPerfilCache);
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
        /// Obtiene la lista de menús asociados a un perfil específico desde la caché.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idPefil">Identificador del perfil para el cual se desean obtener los menús.</param>
        /// <returns>Lista de entidades <see cref="MenuPerfilEntity"/> asociadas al perfil especificado.</returns>
        public List<MenuPerfilEntity> ObtenerMenuPerfilPorIdPerfil(string traceId, int idPefil)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();

            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                return this._menuPerfilCache.Where(m => m.IdPerfil == idPefil && m.Activo).ToList();
                
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
