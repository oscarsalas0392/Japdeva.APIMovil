
using Japdeva.APIMovil.Colas.Services.ColaService;
using Japdeva.APIMovil.Colas.Services.EstadoMensajeService;
using Japdeva.APIMovil.Colas.Services.PrioridadService;
using Japdeva.APIMovil.Common.Extensions;

namespace Japdeva.APIMovil.Colas.BackgroundServices
{
    /// <summary>
    /// Parámetros de configuración para el servicio en segundo plano de colas.
    /// </summary>
    public class ParametrosBackGroundService : BackgroundService
    {
        private readonly ILogger<ParametrosBackGroundService> _logger;
        private readonly IPrioridadService _prioridadesService;
        private readonly IEstadoMensajeService _estadoMensajeService;
        private readonly IColaService _colaService;
        private const int DELAY_MINUTES = 10;
        private const string TRACE_ID = "N/A";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ParametrosBackGroundService"/>.
        /// </summary>
        /// <param name="logger">El logger para registrar eventos del servicio.</param>
        /// <param name="prioridadesService">El servicio de prioridades.</param>
        /// <param name="estadoMensajeService">El servicio de estados de mensajes.</param>
        /// <param name="colaService">El servicio de colas.</param>
        public ParametrosBackGroundService(ILogger<ParametrosBackGroundService> logger, IPrioridadService  prioridadesService, IEstadoMensajeService estadoMensajeService, IColaService colaService)
        {
            this._logger = logger;
            this._prioridadesService = prioridadesService;
            this._estadoMensajeService = estadoMensajeService;
            this._colaService = colaService;
        }

        /// <summary>
        /// Ejecuta la lógica del servicio en segundo plano.
        /// </summary>
        /// <param name="stoppingToken">Token de cancelación para detener el servicio.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(TRACE_ID, nombreMetodo);
                while (!stoppingToken.IsCancellationRequested)
                {
                    await this._prioridadesService.LlenarCachePrioridadesAsync(TRACE_ID);
                    await this._estadoMensajeService.LlenarCacheEstadosMensajeAsync(TRACE_ID);
                    await Task.Delay(TimeSpan.FromMinutes(DELAY_MINUTES), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                this._logger.Error(TRACE_ID, nombreMetodo, ex);
            }
            finally
            {
                this._logger.Fin(TRACE_ID, nombreMetodo);
            }
        }
    }
}