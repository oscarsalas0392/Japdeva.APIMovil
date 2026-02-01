using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Parametros.Entities;

namespace Japdeva.APIMovil.Parametros.Services.TipoMensajeCacheService
{
    /// <summary>
    /// Servicio para gestionar la caché de tipos de mensaje en memoria, obteniendo los datos desde el repositorio
    /// y permitiendo su acceso eficiente.
    /// </summary>
    public class TipoMensajeCacheService : ITipoMensajeCacheService
    {
        private readonly ILogger<TipoMensajeCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<TipoMensajeEntity> _tipoMensajeCache = new List<TipoMensajeEntity>();

        private const int PAGINA_INICIAL = 1;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="TipoMensajeCacheService"/>.
        /// </summary>
        /// <param name="logger">El registrador de eventos para la clase <see cref="TipoMensajeCacheService"/>.</param>
        /// <param name="serviceProvider">El proveedor de servicios para la resolución de dependencias.</param>
        public TipoMensajeCacheService(ILogger<TipoMensajeCacheService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Llena la caché de tipos de mensaje obteniendo los datos desde el repositorio y almacenándolos en memoria.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el registro de logs.</param>
        public async Task LlenarCacheTipoMensajesAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                List<TipoMensajeEntity> listaTipoMensajeCache = new List<TipoMensajeEntity>();
                using var scope = this._serviceProvider.CreateScope();
                var consultarListaEntity = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                int paginaActual = PAGINA_INICIAL;
                int totalPaginas = PAGINA_INICIAL;

                do
                {
                    var respuestaLista = await consultarListaEntity.ConsultarListaAsync<TipoMensajeEntity>(traceId, paginaActual);
                    totalPaginas = respuestaLista.CantidadPaginas;
                    paginaActual++;
                    listaTipoMensajeCache.AddRange(respuestaLista.Lista);
                }
                while (paginaActual <= totalPaginas);

                lock (this._tipoMensajeCache)
                {
                    this._tipoMensajeCache.Clear();
                    this._tipoMensajeCache.AddRange(listaTipoMensajeCache);
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
        /// Obtiene un tipo de mensaje específico por su identificador desde la caché.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el registro de logs.</param>
        /// <param name="id">Identificador del tipo de mensaje a buscar.</param>
        /// <returns>El tipo de mensaje si existe y está activo, null en caso contrario.</returns>
        public TipoMensajeEntity? ObtenerTipoMensajePorId(string traceId, int id)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();

            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                return this._tipoMensajeCache.FirstOrDefault(m => m.Id == id && m.Activo);
                
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
