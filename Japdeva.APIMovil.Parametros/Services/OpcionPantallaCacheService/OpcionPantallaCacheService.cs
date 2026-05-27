using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Parametros.Entities;

namespace Japdeva.APIMovil.Parametros.Services.OpcionPantallaCacheService
{
    /// <summary>
    /// Servicio para gestionar la caché de opciones de pantalla en memoria, obteniendo los datos desde el repositorio
    /// y permitiendo su acceso eficiente.
    /// </summary>
    public class OpcionPantallaCacheService : IOpcionPantallaCacheService
    {
        private readonly ILogger<OpcionPantallaCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<OpcionPantallaEntity> _opcionPantallaCache = new List<OpcionPantallaEntity>();

        private const int PAGINA_INICIAL = 1;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="OpcionPantallaCacheService"/>.
        /// </summary>
        /// <param name="logger">El registrador de eventos para la clase <see cref="OpcionPantallaCacheService"/>.</param>
        /// <param name="serviceProvider">El proveedor de servicios para la resolución de dependencias.</param>
        public OpcionPantallaCacheService(ILogger<OpcionPantallaCacheService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Llena la caché de opciones de pantalla obteniendo los datos desde el repositorio y almacenándolos en memoria.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el registro de logs.</param>
        public async Task LlenarCacheOpcionPantallaAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                List<OpcionPantallaEntity> listaOpcionPantallaCache = new List<OpcionPantallaEntity>();
                var scope = this._serviceProvider.CreateScope();
                var consultarListaEntity = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                int paginaActual = PAGINA_INICIAL;
                int totalPaginas = PAGINA_INICIAL;

                do
                {
                    var respuestaLista = await consultarListaEntity.ConsultarListaAsync<OpcionPantallaEntity>(traceId, paginaActual);
                    totalPaginas = respuestaLista.CantidadPaginas;
                    paginaActual++;
                    listaOpcionPantallaCache.AddRange(respuestaLista.Lista);
                }
                while (paginaActual <= totalPaginas);

                lock (this._opcionPantallaCache)
                {
                    this._opcionPantallaCache.Clear();
                    this._opcionPantallaCache.AddRange(listaOpcionPantallaCache);
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
        /// Obtiene las opciones de pantalla activas que coinciden con los identificadores indicados.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el registro de logs.</param>
        /// <param name="listaIds">Lista de identificadores a buscar.</param>
        /// <returns>Lista de entidades que cumplen con los criterios.</returns>
        public List<OpcionPantallaEntity> ObtenerOpcionPantallaPorId(string traceId, List<int> listaIds)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                return this._opcionPantallaCache.Where(o => listaIds.Contains(o.Id) && o.Activo).ToList();
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
