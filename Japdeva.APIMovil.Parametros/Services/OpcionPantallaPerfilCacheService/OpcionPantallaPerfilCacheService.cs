using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Parametros.Entities;

namespace Japdeva.APIMovil.Parametros.Services.OpcionPantallaPerfilCacheService
{
    /// <summary>
    /// Servicio para gestionar la caché de opciones de pantalla por perfil en memoria, obteniendo los datos desde el repositorio
    /// y permitiendo su acceso eficiente.
    /// </summary>
    public class OpcionPantallaPerfilCacheService : IOpcionPantallaPerfilCacheService
    {
        private readonly ILogger<OpcionPantallaPerfilCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<OpcionPantallaPerfilEntity> _opcionPantallaPerfilCache = new List<OpcionPantallaPerfilEntity>();

        private const int PAGINA_INICIAL = 1;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="OpcionPantallaPerfilCacheService"/>.
        /// </summary>
        /// <param name="logger">Instancia del registrador de logs.</param>
        /// <param name="serviceProvider">Proveedor de servicios para la resolución de dependencias.</param>
        public OpcionPantallaPerfilCacheService(ILogger<OpcionPantallaPerfilCacheService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Llena la caché de opciones de pantalla por perfil obteniendo los datos desde el repositorio y almacenándolos en memoria.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        public async Task LlenarCacheOpcionPantallaPerfilAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                List<OpcionPantallaPerfilEntity> listaOpcionPantallaPerfilCache = new List<OpcionPantallaPerfilEntity>();
                var scope = this._serviceProvider.CreateScope();
                var consultarListaEntity = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                int paginaActual = PAGINA_INICIAL;
                int totalPaginas = PAGINA_INICIAL;

                do
                {
                    var respuestaLista = await consultarListaEntity.ConsultarListaAsync<OpcionPantallaPerfilEntity>(traceId, paginaActual);
                    totalPaginas = respuestaLista.CantidadPaginas;
                    paginaActual++;
                    listaOpcionPantallaPerfilCache.AddRange(respuestaLista.Lista);
                }
                while (paginaActual <= totalPaginas);

                lock (this._opcionPantallaPerfilCache)
                {
                    this._opcionPantallaPerfilCache.Clear();
                    this._opcionPantallaPerfilCache.AddRange(listaOpcionPantallaPerfilCache);
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
        /// Obtiene las opciones de pantalla asociadas a un perfil específico desde la caché.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idPerfil">Identificador del perfil para el cual se desean obtener las opciones.</param>
        /// <returns>Lista de entidades <see cref="OpcionPantallaPerfilEntity"/> asociadas al perfil especificado.</returns>
        public List<OpcionPantallaPerfilEntity> ObtenerOpcionPantallaPerfilPorIdPerfil(string traceId, int idPerfil)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                return this._opcionPantallaPerfilCache.Where(o => o.IdPerfil == idPerfil && o.Activo).ToList();
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
