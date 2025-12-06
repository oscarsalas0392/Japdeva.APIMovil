using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.OrdenProcesoCacheService
{
    /// <summary>
    /// Servicio de cache para la gestión de órdenes de proceso en memoria.
    /// Proporciona acceso rápido a la configuración del workflow de reclamos,
    /// almacenando en cache las etapas y secuencias del proceso de atención para optimizar el rendimiento.
    /// </summary>
    public class OrdenProcesoCacheService : IOrdenProcesoCacheService
    {
        private readonly ILogger<OrdenProcesoCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<OrdenProcesoEntity> _ordenProcesoEntityCache = new List<OrdenProcesoEntity>();
        private const int PAGINA_INICIAL = 1;
        private const bool ESTADO_ACTIVO = true;

        /// <summary>
        /// Inicializa una nueva instancia del servicio de cache de órdenes de proceso.
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias y gestión de scopes.</param>
        /// <param name="logger">Logger para registro de eventos y errores del servicio de cache.</param>
        public OrdenProcesoCacheService(IServiceProvider serviceProvider, ILogger<OrdenProcesoCacheService> logger)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Carga el cache con todas las órdenes de proceso activas desde la base de datos.
        /// Actualiza completamente el cache con la configuración más reciente del workflow de reclamos.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        public async Task LlenarCacheOrdenProcesoAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                var ordenProcesos = await consultarRepository.ConsultarListaAsync<OrdenProcesoEntity>(
                    traceId, PAGINA_INICIAL, p => p.Activo == ESTADO_ACTIVO);
                if (ordenProcesos is null || !ordenProcesos.Lista.Any()) return;
                lock (this._ordenProcesoEntityCache)
                {
                    this._ordenProcesoEntityCache.Clear();
                    this._ordenProcesoEntityCache.AddRange(ordenProcesos.Lista);
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
        /// Obtiene una orden de proceso específica desde el cache en memoria.
        /// Realiza una búsqueda thread-safe en el cache para encontrar la configuración de una etapa del workflow.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idOrdenProceso">Identificador de la orden de proceso a buscar.</param>
        /// <returns>La entidad de orden de proceso si se encuentra y está activa, null en caso contrario.</returns>
        public OrdenProcesoEntity? ObtenerOrdenProceso(string traceId, int idOrdenProceso)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                lock (this._ordenProcesoEntityCache)
                {
                    return this._ordenProcesoEntityCache.FirstOrDefault(p => p.Id == idOrdenProceso && p.Activo == ESTADO_ACTIVO);
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
