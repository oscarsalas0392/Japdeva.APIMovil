using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.EnvioCorreos.Services.ProcesarCorreosPendientesService;

namespace Japdeva.APIMovil.EnvioCorreos.BackgroundServices
{
    /// <summary>
    /// Servicio de fondo que procesa periódicamente la cola de correos pendientes de envío.
    /// </summary>
    public class EnviarCorreosBackGroundService : BackgroundService
    {
        private readonly ILogger<EnviarCorreosBackGroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const string TRACE_ID_BACKGROUND = "BACKGROUND_ENVIAR_CORREOS";
        private const int DELAY_MINUTOS = 2;

        /// <summary>
        /// Inicializa una nueva instancia de EnviarCorreosBackGroundService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos del servicio de fondo.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias.</param>
        public EnviarCorreosBackGroundService(ILogger<EnviarCorreosBackGroundService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Ejecuta el ciclo de procesamiento de correos pendientes de forma continua.
        /// </summary>
        /// <param name="stoppingToken">Token de cancelación para detener el servicio.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(TRACE_ID_BACKGROUND, nombreMetodo);
                while (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = this._serviceProvider.CreateScope();
                    var procesarService = scope.ServiceProvider.GetRequiredService<IProcesarCorreosPendientesService>();
                    await procesarService.ProcesarPendientesAsync(TRACE_ID_BACKGROUND);
                    await Task.Delay(TimeSpan.FromMinutes(DELAY_MINUTOS), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                this._logger.Error(TRACE_ID_BACKGROUND, nombreMetodo, ex);
            }
            finally
            {
                this._logger.Fin(TRACE_ID_BACKGROUND, nombreMetodo);
            }
        }
    }
}
