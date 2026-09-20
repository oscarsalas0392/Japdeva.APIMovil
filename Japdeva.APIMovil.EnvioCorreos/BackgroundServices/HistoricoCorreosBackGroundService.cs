using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.EnvioCorreos.Services.EnviarCorreoHistoricoService;

namespace Japdeva.APIMovil.EnvioCorreos.BackgroundServices
{
    /// <summary>
    /// Servicio de fondo que mueve periódicamente al histórico los correos enviados o con intentos agotados.
    /// </summary>
    public class HistoricoCorreosBackGroundService : BackgroundService
    {
        private readonly ILogger<HistoricoCorreosBackGroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const string TRACE_ID_BACKGROUND = "BACKGROUND_HISTORICO_CORREOS";
        private const int DELAY_MINUTOS = 30;

        /// <summary>
        /// Inicializa una nueva instancia de HistoricoCorreosBackGroundService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos del servicio de fondo.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias.</param>
        public HistoricoCorreosBackGroundService(ILogger<HistoricoCorreosBackGroundService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Ejecuta el ciclo de limpieza y movimiento al histórico de forma continua.
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
                    var historicoService = scope.ServiceProvider.GetRequiredService<IEnviarCorreoHistoricoService>();
                    await historicoService.EnviarHistoricoAsync(TRACE_ID_BACKGROUND);
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
