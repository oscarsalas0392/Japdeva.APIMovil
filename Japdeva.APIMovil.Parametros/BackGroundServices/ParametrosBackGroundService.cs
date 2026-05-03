using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Parametros.Services.MenuCacheService;
using Japdeva.APIMovil.Parametros.Services.MenuPerfilCacheService;
using Japdeva.APIMovil.Parametros.Services.PantallaCacheService;
using Japdeva.APIMovil.Parametros.Services.ParametroCacheService;
using Japdeva.APIMovil.Parametros.Services.PlantillaCorreoCacheService;

namespace Japdeva.APIMovil.Parametros.BackgroundServices
{
    /// <summary>
    /// Servicio de fondo para la inicialización y gestión de parámetros del sistema.
    /// Se ejecuta como un servicio hospedado que se encarga de cargar y mantener actualizados
    /// los caches de configuración y catálogos generales de la aplicación.
    /// </summary>
    public class ParametrosBackGroundService : BackgroundService
    {
        private readonly ILogger<ParametrosBackGroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const int TIEMPO_ESPERA_ENTRE_EJECUCIONES = 300000; // 5 minutos
        private const string TRACE_ID_BACKGROUND = "BACKGROUND_PARAMETROS";

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
        /// Carga los caches de parámetros y configuraciones del sistema en intervalos regulares.
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
        /// Carga todos los caches de parámetros generales del sistema.
        /// Incluye pantallas, menús, perfiles de menú, mensajes y tipos de mensaje.
        /// </summary>
        public async Task CargarCachesParametrosAsync()
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(TRACE_ID_BACKGROUND, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();

                var pantallaCache = scope.ServiceProvider.GetRequiredService<IPantallaCacheService>();
                var menuCache = scope.ServiceProvider.GetRequiredService<IMenuCacheService>();
                var menuPerfilCache = scope.ServiceProvider.GetRequiredService<IMenuPerfilCacheService>();
                var parametroCache = scope.ServiceProvider.GetRequiredService<IParametroCacheService>();
                var plantillaCorreoCache = scope.ServiceProvider.GetRequiredService<IPlantillaCorreoCacheService>();

                Task tareaPantallaCache = pantallaCache.LlenarCachePantallaAsync(TRACE_ID_BACKGROUND);
                Task tareaMenuCache = menuCache.LlenarCacheMenuAsync(TRACE_ID_BACKGROUND);
                Task tareaMenuPerfilCache = menuPerfilCache.LlenarCacheMenuPerfilAsync(TRACE_ID_BACKGROUND);
                Task tareaParametroCache = parametroCache.LlenarCacheParametrosAsync(TRACE_ID_BACKGROUND);
                Task tareaPlantillaCorreoCache = plantillaCorreoCache.LlenarCachePlantillaCorreosAsync(TRACE_ID_BACKGROUND);

                await Task.WhenAll(
                    tareaPantallaCache,
                    tareaMenuCache,
                    tareaMenuPerfilCache,
                    tareaParametroCache,
                    tareaPlantillaCorreoCache);

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
