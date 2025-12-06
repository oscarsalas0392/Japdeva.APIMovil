using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Reclamos.Services.DevolucionProcesoCacheService;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoCacheService;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoOrdenProcesoCacheService;
using Japdeva.APIMovil.Reclamos.Services.EstadoReclamoCacheService;
using Japdeva.APIMovil.Reclamos.Services.OrdenProcesoCacheService;

namespace Japdeva.APIMovil.Reclamos.BackgroundServices
{
    /// <summary>
    /// Servicio de fondo para la inicialización y gestión de parámetros del módulo de Reclamos.
    /// Se ejecuta como un servicio hospedado que se encarga de cargar y mantener actualizados
    /// los caches de configuración y catálogos del sistema de reclamos.
    /// </summary>
    public class ParametrosBackGroundService : BackgroundService
    {
        private readonly ILogger<ParametrosBackGroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const int TIEMPO_ESPERA_ENTRE_EJECUCIONES = 300000; // 5 minutos
        private const string TRACE_ID_BACKGROUND = "BACKGROUND_PARAMETROS_RECLAMOS";

        /// <summary>
        /// Inicializa una nueva instancia del servicio de fondo de parámetros.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos del servicio de fondo.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias.</param>
        public ParametrosBackGroundService(ILogger<ParametrosBackGroundService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Ejecuta la lógica principal del servicio de fondo de forma continua.
        /// Carga los caches de parámetros y configuraciones del sistema de reclamos en intervalos regulares.
        /// </summary>
        /// <param name="stoppingToken">Token de cancelación para detener el servicio de forma controlada.</param>
        /// <returns>Una tarea que representa la ejecución continua del servicio de fondo.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(TRACE_ID_BACKGROUND, nombreMetodo);
                
                while (!stoppingToken.IsCancellationRequested)
                {
                    await CargarCachesParametrosAsync();
                    await Task.Delay(TIEMPO_ESPERA_ENTRE_EJECUCIONES, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                this._logger.Error(TRACE_ID_BACKGROUND, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(TRACE_ID_BACKGROUND, nombreMetodo);
            }
        }

        /// <summary>
        /// Carga todos los caches de parámetros y configuraciones del módulo de Reclamos.
        /// Incluye estados, órdenes de proceso, devoluciones y relaciones entre entidades.
        /// </summary>
        public async Task CargarCachesParametrosAsync()
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(TRACE_ID_BACKGROUND, nombreMetodo);        
                using var scope = this._serviceProvider.CreateScope();

                var estadoReclamoCache = scope.ServiceProvider.GetRequiredService<IEstadoReclamoCacheService>();
                var estadoDetalleReclamoCache = scope.ServiceProvider.GetRequiredService<IEstadoDetalleReclamoCacheService>();
                var ordenProcesoCache = scope.ServiceProvider.GetRequiredService<IOrdenProcesoCacheService>();
                var devolucionProcesoCache = scope.ServiceProvider.GetRequiredService<IDevolucionProcesoCacheService>();
                var estadoDetalleOrdenCache = scope.ServiceProvider.GetRequiredService<IEstadoDetalleReclamoOrdenProcesoCacheService>();

                Task tareaEstadoReclamoCache = estadoReclamoCache.LlenarCacheEstadoReclamoAsync(TRACE_ID_BACKGROUND);
                Task tareaEstadoDetalleReclamoCache = estadoDetalleReclamoCache.LlenarCacheEstadoDetalleReclamoAsync(TRACE_ID_BACKGROUND);
                Task tareaOrdenProcesoCache = ordenProcesoCache.LlenarCacheOrdenProcesoAsync(TRACE_ID_BACKGROUND);
                Task tareaDevolucionProcesoCache = devolucionProcesoCache.LlenarCacheDevolucionProcesoAsync(TRACE_ID_BACKGROUND);
                Task tareaEstadoDetalleOrdenCache = estadoDetalleOrdenCache.LlenarCacheEstadoDetalleReclamoOrdenProcesoAsync(TRACE_ID_BACKGROUND);

                await Task.WhenAll(
                    tareaEstadoReclamoCache,
                    tareaEstadoDetalleReclamoCache,
                    tareaOrdenProcesoCache,
                    tareaDevolucionProcesoCache,
                    tareaEstadoDetalleOrdenCache);

            }
            catch (Exception ex)
            {
                this._logger.Error(TRACE_ID_BACKGROUND, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(TRACE_ID_BACKGROUND, nombreMetodo);
            }
        }
    }
}
