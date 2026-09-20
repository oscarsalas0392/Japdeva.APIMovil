using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Parametros.Entities;

namespace Japdeva.APIMovil.Parametros.Services.PantallaCacheService
{
    /// <summary>
    /// Servicio para gestionar la caché de pantallas en memoria, obteniendo los datos desde el repositorio
    /// y permitiendo su acceso eficiente.
    /// </summary>
    public class PantallaCacheService : IPantallaCacheService
    {
        private readonly ILogger<PantallaCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<PantallaEntity> _pantallaCache = new List<PantallaEntity>();

        private const int PAGINA_INICIAL = 1;


        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="PantallaCacheService"/>.
        /// </summary>
        /// <param name="logger">Instancia del registrador de logs.</param>
        /// <param name="serviceProvider">Proveedor de servicios para la obtención de dependencias.</param>
        public PantallaCacheService(ILogger<PantallaCacheService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Llena la caché de pantallas obteniendo la información desde el repositorio remoto.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        public async Task LlenarCachePantallaAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                List<PantallaEntity> listaPantalla= new List<PantallaEntity>();
                using var scope = this._serviceProvider.CreateScope();
                var consultarListaEntity = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                int paginaActual = PAGINA_INICIAL;
                int totalPaginas = PAGINA_INICIAL;

                do
                {
                    var respuestaLista = await consultarListaEntity.ConsultarListaAsync<PantallaEntity>(traceId, paginaActual);
                    totalPaginas = respuestaLista.CantidadPaginas;
                    paginaActual++;
                    listaPantalla.AddRange(respuestaLista.Lista);
                }
                while (paginaActual <= totalPaginas);

                lock (this._pantallaCache)
                {
                    this._pantallaCache.Clear();
                    this._pantallaCache.AddRange(listaPantalla);
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
        /// Obtiene las pantallas por una lista de IDs.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="listaIds">Lista de IDs de pantallas a obtener.</param>
        /// <returns>Lista de pantallas activas que coinciden con los IDs proporcionados.</returns>
        public List<PantallaEntity> ObtenerPantallaPorListaId(string traceId, List<int> listaIds)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();

            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                return this._pantallaCache.Where(m => listaIds.Contains(m.Id) && m.Activo).ToList();
                
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
