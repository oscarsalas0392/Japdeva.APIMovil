using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.DevolucionProcesoCacheService
{
    /// <summary>
    /// Servicio de cache para la gestión de configuraciones de devolución de proceso en memoria.
    /// Proporciona acceso rápido a las reglas de devolución del workflow de reclamos,
    /// optimizando las consultas a las configuraciones que definen cuándo y hacia dónde devolver un proceso.
    /// </summary>
    public class DevolucionProcesoCacheService : IDevolucionProcesoCacheService
    {
        private readonly ILogger<DevolucionProcesoCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<DevolucionProcesoEntity> _devolucionProcesoEntityCache = new List<DevolucionProcesoEntity>();
        private const int PAGINA_INICIAL = 1;
        private const bool ESTADO_ACTIVO = true;

        /// <summary>
        /// Inicializa una nueva instancia del servicio de cache de devoluciones de proceso.
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias y gestión de scopes.</param>
        /// <param name="logger">Logger para registro de eventos y errores del servicio de cache.</param>
        public DevolucionProcesoCacheService(IServiceProvider serviceProvider, ILogger<DevolucionProcesoCacheService> logger)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Carga el cache con todas las configuraciones de devolución de proceso activas desde la base de datos.
        /// Actualiza completamente el cache con las reglas más recientes de devolución del workflow.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        public async Task LlenarCacheDevolucionProcesoAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                var devolucionProcesos = await consultarRepository.ConsultarListaAsync<DevolucionProcesoEntity>(
                    traceId, PAGINA_INICIAL, p => p.Activo == ESTADO_ACTIVO);
                if (devolucionProcesos is null || !devolucionProcesos.Lista.Any()) return;
                lock (this._devolucionProcesoEntityCache)
                {
                    this._devolucionProcesoEntityCache.Clear();
                    this._devolucionProcesoEntityCache.AddRange(devolucionProcesos.Lista);
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
        /// Obtiene una configuración de devolución de proceso específica desde el cache en memoria.
        /// Realiza una búsqueda thread-safe para encontrar las reglas de devolución aplicables a un proceso específico.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idDevolucionProceso">Identificador de la configuración de devolución de proceso a buscar.</param>
        /// <returns>La entidad de devolución de proceso si se encuentra y está activa, null en caso contrario.</returns>
        public DevolucionProcesoEntity? ObtenerDevolucionProceso(string traceId, int idDevolucionProceso)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                lock (this._devolucionProcesoEntityCache)
                {
                    return this._devolucionProcesoEntityCache.FirstOrDefault(p => p.Id == idDevolucionProceso && p.Activo == ESTADO_ACTIVO);
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
