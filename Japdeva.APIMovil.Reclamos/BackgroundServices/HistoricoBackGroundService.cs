using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Reclamos.Services.ValidarEnvioReclamoHistoricoService;

namespace Japdeva.APIMovil.Reclamos.BackgroundServices
{
    /// <summary>
    /// Servicio en segundo plano encargado de validar y procesar el envío histórico de reclamos de forma periódica.
    /// </summary>
    public class HistoricoBackGroundService : BackgroundService
    {
        private readonly ILogger<HistoricoBackGroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
      
        private const string TRACE_ID_BACKGROUND = "BACKGROUND_HISTORICO";
        private const int DELAY_MINUTOS= 10;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="HistoricoBackGroundService"/>.
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios para la obtención de dependencias.</param>
        /// <param name="logger">Instancia del registrador para el servicio.</param>
        public HistoricoBackGroundService(IServiceProvider serviceProvider, ILogger<HistoricoBackGroundService> logger)
            => (this._serviceProvider, this._logger) = (serviceProvider, logger);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
           string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(TRACE_ID_BACKGROUND, nombreMetodo);
                while (!stoppingToken.IsCancellationRequested)
                {
                    using var scope = this._serviceProvider.CreateScope();
                    var validarHistorico = scope.ServiceProvider.GetRequiredService<IValidarEnvioReclamoHistoricoService>();
                    await validarHistorico.ValidarEnvioReclamoHistoricoAsync(TRACE_ID_BACKGROUND);
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
