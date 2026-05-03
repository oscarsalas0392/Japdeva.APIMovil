using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Usuarios.Services.DepartamentoCacheService;
using Japdeva.APIMovil.Usuarios.Services.RolCacheService;
using Japdeva.APIMovil.Usuarios.Services.TipoCedulaCacheService;

namespace Japdeva.APIMovil.Usuarios.BackgroundServices
{
    /// <summary>
    /// Servicio de fondo para la inicialización y gestión de parámetros del módulo de Usuarios.
    /// Carga y mantiene actualizados los cachés de roles, tipos de cédula y departamentos.
    /// </summary>
    public class UsuariosBackGroundService : BackgroundService
    {
        private readonly ILogger<UsuariosBackGroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const int TIEMPO_ESPERA_ENTRE_EJECUCIONES = 300000;
        private const string TRACE_ID_BACKGROUND = "BACKGROUND_PARAMETROS_USUARIOS";

        /// <summary>
        /// Inicializa una nueva instancia del servicio de fondo de parámetros de Usuarios.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos del servicio de fondo.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias.</param>
        public UsuariosBackGroundService(ILogger<UsuariosBackGroundService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Ejecuta la lógica principal del servicio de fondo de forma continua.
        /// Carga los cachés de parámetros del módulo de Usuarios en intervalos regulares.
        /// </summary>
        /// <param name="stoppingToken">Token de cancelación para detener el servicio de forma controlada.</param>
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
        /// Carga todos los cachés de parámetros del módulo de Usuarios en paralelo.
        /// </summary>
        public async Task CargarCachesParametrosAsync()
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(TRACE_ID_BACKGROUND, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();

                var rolCache = scope.ServiceProvider.GetRequiredService<IRolCacheService>();
                var tipoCedulaCache = scope.ServiceProvider.GetRequiredService<ITipoCedulaCacheService>();
                var departamentoCache = scope.ServiceProvider.GetRequiredService<IDepartamentoCacheService>();

                Task tareaRolCache = rolCache.LlenarCacheRolesAsync(TRACE_ID_BACKGROUND);
                Task tareaTipoCedulaCache = tipoCedulaCache.LlenarCacheTiposCedulaAsync(TRACE_ID_BACKGROUND);
                Task tareaDepartamentoCache = departamentoCache.LlenarCacheDepartamentosAsync(TRACE_ID_BACKGROUND);

                await Task.WhenAll(tareaRolCache, tareaTipoCedulaCache, tareaDepartamentoCache);
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
