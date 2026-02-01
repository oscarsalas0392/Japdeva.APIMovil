using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Parametros.Entities;

namespace Japdeva.APIMovil.Parametros.Services.MensajeCacheService
{
    /// <summary>
    /// Servicio para gestionar la caché de mensajes en la aplicación móvil de JAPDEVA.
    /// </summary>
    public class MensajeCacheService : IMensajeCacheService
    {
        private readonly ILogger<MensajeCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<MensajeEntity> _mensajesCache = new List<MensajeEntity>();

        private const int PAGINA_INICIAL = 1;
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="MensajeCacheService"/>.
        /// </summary>
        /// <param name="logger">El registrador de eventos para la clase <see cref="MensajeCacheService"/>.</param>
        /// <param name="serviceProvider">El proveedor de servicios para la resolución de dependencias.</param>
        public MensajeCacheService(ILogger<MensajeCacheService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Llena la caché de mensajes obteniéndolos desde el repositorio remoto de mensajes.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        public async Task LlenarCacheMensajesAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                List<MensajeEntity> listaMensajesCache = new List<MensajeEntity>();
                this._logger.Inicio(traceId, nombreMetodo);

                using var scope = this._serviceProvider.CreateScope();
                var consultarListaEntity = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                int paginaActual = PAGINA_INICIAL;
                int totalPaginas = PAGINA_INICIAL;

                do
                {
                    var respuestaLista = await consultarListaEntity.ConsultarListaAsync<MensajeEntity>(traceId, paginaActual);
                    totalPaginas = respuestaLista.CantidadPaginas;
                    paginaActual++;
                    listaMensajesCache.AddRange(respuestaLista.Lista);
                }
                while (paginaActual <= totalPaginas);

                lock (this._mensajesCache)
                {
                    this._mensajesCache.Clear();
                    this._mensajesCache.AddRange(listaMensajesCache);
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
        /// Obtiene los mensajes almacenados en caché para una pantalla específica y que estén activos.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idPantalla">Identificador de la pantalla para filtrar los mensajes.</param>
        /// <returns>Lista de mensajes activos asociados a la pantalla especificada.</returns>
        public List<MensajeEntity> ObtenerMensajes(string traceId, int idPantalla)
        {
           string nombreMetodo = this.ObtenerNombreMetodo();

            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                return this._mensajesCache.Where(m => m.IdPantalla == idPantalla && m.Activo).ToList();
                
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
