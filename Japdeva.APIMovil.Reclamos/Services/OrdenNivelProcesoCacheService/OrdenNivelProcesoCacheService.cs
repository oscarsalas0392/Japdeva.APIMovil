using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.OrdenNivelProcesoCacheService
{
    /// <summary>
    /// Servicio para gestionar el caché de entidades <see cref="OrdenNivelProcesoEntity"/> relacionadas con los niveles de proceso de órdenes.
    /// Permite llenar y consultar el caché de niveles de proceso activos.
    /// </summary>
    public class OrdenNivelProcesoCacheService : IOrdenNivelProcesoCacheService
    {
        private readonly ILogger<OrdenNivelProcesoCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<OrdenNivelProcesoEntity> _orderNivelProcesoEntityCache = new List<OrdenNivelProcesoEntity>();
        private const int PAGINA_INICIAL = 1;
        private const bool ESTADO_ACTIVO = true;

        private const string MENSAJE_ERROR_NO_SE_PUEDEN_OBTENER_LOS_DATOS_DE_ORDEN_NIVEL_PROCESO = "No se pudieron obtener los datos de OrdenNivelProcesoEntity para llenar el caché.";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="OrdenNivelProcesoCacheService"/>.
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios para la obtención de dependencias.</param>
        /// <param name="logger">Instancia del registrador para el servicio.</param>
        public OrdenNivelProcesoCacheService(IServiceProvider serviceProvider, ILogger<OrdenNivelProcesoCacheService> logger)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Llena el caché de entidades <see cref="OrdenNivelProcesoEntity"/> activas relacionadas con los niveles de proceso de órdenes.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        public async Task LlenarCacheOrdenNivelProcesoAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
        
                this._logger.Inicio(traceId, nombreMetodo);
                int pagina = PAGINA_INICIAL;
                int totalPaginas = PAGINA_INICIAL;
                List<OrdenNivelProcesoEntity> listaCache = new List<OrdenNivelProcesoEntity>();
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();

                do
                {
                    var resultadoPagina = await consultarRepository.ConsultarListaAsync<OrdenNivelProcesoEntity>(
                        traceId, pagina, p => p.Activo == ESTADO_ACTIVO);

                    if (resultadoPagina is null || !resultadoPagina.Lista.Any())
                    {
                       throw new Exception(MENSAJE_ERROR_NO_SE_PUEDEN_OBTENER_LOS_DATOS_DE_ORDEN_NIVEL_PROCESO);
                    }

                    listaCache.AddRange(resultadoPagina.Lista);
                    totalPaginas = resultadoPagina.CantidadPaginas;
                    pagina++;

                } while (pagina <= totalPaginas);

                if (listaCache is null || !listaCache.Any()) return;
                lock (this._orderNivelProcesoEntityCache)
                {
                    this._orderNivelProcesoEntityCache.Clear();
                    this._orderNivelProcesoEntityCache.AddRange(listaCache);
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
        /// Obtiene una lista de entidades <see cref="OrdenNivelProcesoEntity"/> del caché que corresponden al identificador del nivel superior especificado.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idNivelSuperior">Identificador del nivel superior para filtrar las entidades.</param>
        /// <returns>Lista de entidades <see cref="OrdenNivelProcesoEntity"/> que coinciden con el nivel superior proporcionado.</returns>
        public List<OrdenNivelProcesoEntity> ObtenerOrdenesNivelesProcesoCache(string traceId, int idNivelSuperior)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                lock (this._orderNivelProcesoEntityCache)
                {
                    return this._orderNivelProcesoEntityCache.Where(x=>x.IdNivelSuperior == idNivelSuperior).ToList();
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
        /// Obtiene una entidad <see cref="OrdenNivelProcesoEntity"/> del caché que corresponde a los identificadores de nivel superior e inferior especificados.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idNivelSuperior">Identificador del nivel superior para filtrar la entidad.</param>
        /// <param name="idNivelInferior">Identificador del nivel inferior para filtrar la entidad.</param>
        /// <returns>Entidad <see cref="OrdenNivelProcesoEntity"/> que coincide con los identificadores proporcionados, o <c>null</c> si no se encuentra.</returns>
        public OrdenNivelProcesoEntity? ObtenerOrdenNivelProcesoCache(string traceId, int idNivelSuperior, int idNivelInferior)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                lock (this._orderNivelProcesoEntityCache)
                {
                    return this._orderNivelProcesoEntityCache.FirstOrDefault(
                        x => x.IdNivelSuperior == idNivelSuperior 
                             && x.IdNivelInferior == idNivelInferior 
                             && x.Activo == ESTADO_ACTIVO);
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

    }
}
