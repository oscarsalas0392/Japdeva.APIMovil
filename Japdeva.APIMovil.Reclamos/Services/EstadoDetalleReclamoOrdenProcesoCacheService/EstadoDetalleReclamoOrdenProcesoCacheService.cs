using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoOrdenProcesoCacheService
{
    /// <summary>
    /// Servicio de cache para la gestión de relaciones entre estados de detalle y órdenes de proceso en memoria.
    /// Proporciona acceso rápido a las configuraciones de workflow que definen qué estados son válidos
    /// para cada etapa del proceso de reclamos, optimizando las validaciones de flujo de trabajo.
    /// </summary>
    public class EstadoDetalleReclamoOrdenProcesoCacheService : IEstadoDetalleReclamoOrdenProcesoCacheService
    {
        private readonly ILogger<EstadoDetalleReclamoOrdenProcesoCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<EstadoDetalleReclamoOrdenProcesoEntity> _estadoDetalleReclamoOrdenProcesoEntityCache = new List<EstadoDetalleReclamoOrdenProcesoEntity>();
        private const int PAGINA_INICIAL = 1;
        private const bool ESTADO_ACTIVO = true;

        /// <summary>
        /// Inicializa una nueva instancia del servicio de cache de relaciones estado-orden de proceso.
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias y gestión de scopes.</param>
        /// <param name="logger">Logger para registro de eventos y errores del servicio de cache.</param>
        public EstadoDetalleReclamoOrdenProcesoCacheService(IServiceProvider serviceProvider, ILogger<EstadoDetalleReclamoOrdenProcesoCacheService> logger)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Carga el cache con todas las relaciones activas entre estados de detalle y órdenes de proceso desde la base de datos.
        /// Actualiza completamente el cache con las configuraciones más recientes del workflow de validación.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        public async Task LlenarCacheEstadoDetalleReclamoOrdenProcesoAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                var estadoDetalleOrdenProcesos = await consultarRepository.ConsultarListaAsync<EstadoDetalleReclamoOrdenProcesoEntity>(
                    traceId, PAGINA_INICIAL, p => p.Activo == ESTADO_ACTIVO);
                if (estadoDetalleOrdenProcesos is null || !estadoDetalleOrdenProcesos.Lista.Any()) return;
                lock (this._estadoDetalleReclamoOrdenProcesoEntityCache)
                {
                    this._estadoDetalleReclamoOrdenProcesoEntityCache.Clear();
                    this._estadoDetalleReclamoOrdenProcesoEntityCache.AddRange(estadoDetalleOrdenProcesos.Lista);
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
        /// Obtiene una relación específica entre estado de detalle y orden de proceso desde el cache en memoria.
        /// Realiza una búsqueda thread-safe para encontrar las configuraciones de workflow aplicables a una orden específica.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idOrdenProceso">Identificador de la orden de proceso para buscar sus estados válidos.</param>
        /// <returns>La entidad de relación estado-orden si se encuentra y está activa, null en caso contrario.</returns>
        public EstadoDetalleReclamoOrdenProcesoEntity? ObtenerEstadoDetalleReclamoOrdenProceso(string traceId, int idOrdenProceso)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                lock (this._estadoDetalleReclamoOrdenProcesoEntityCache)
                {
                    return this._estadoDetalleReclamoOrdenProcesoEntityCache.FirstOrDefault(p => p.IdOrdenProceso == idOrdenProceso && p.Activo == ESTADO_ACTIVO);
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
