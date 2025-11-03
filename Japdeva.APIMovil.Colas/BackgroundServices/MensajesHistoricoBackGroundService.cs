using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Colas.Models;
using Japdeva.APIMovil.Colas.Services.GuardarMensajesHistoricoService;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;

namespace Japdeva.APIMovil.Colas.BackgroundServices
{
    /// <summary>
    /// Background service for processing historic messages.
    /// </summary>
    public class MensajesHistoricoBackgroundService : BackgroundService
    {
        private readonly ILogger<MensajesHistoricoBackgroundService> _logger;
        private readonly IGuardarMensajesHistoricoService _guardarMensajesHistoricoService;
        private readonly IConsultarRepository _consultarRepository;
        private const string TRACE_ID = "N/A";
        private const int CANTIDAD_MENSAJES_MINIMA = 0;
        private const int UMBRAL_MUCHA_CARGA = 5000;
        private const int UMBRAL_CARGA_NORMAL = 1000;
        private const int UMBRAL_POCA_CARGA = 100;
        private const int DELAY_MUCHA_CARGA = 1;
        private const int DELAY_CARGA_NORMAL = 2;
        private const int DELAY_POCA_CARGA = 5;
        private const int DELAY_SIN_CARGA = 10;

        /// <summary>
        /// Initializes a new instance of the <see cref="MensajesHistoricoBackgroundService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="guardarMensajesHistoricoService">The service for saving historic messages.</param>
        /// <param name="consultarRepository">The repository for querying data.</param>
        public MensajesHistoricoBackgroundService(ILogger<MensajesHistoricoBackgroundService> logger, IGuardarMensajesHistoricoService guardarMensajesHistoricoService, IConsultarRepository consultarRepository)
        {
            _logger = logger;
            _guardarMensajesHistoricoService = guardarMensajesHistoricoService;
            _consultarRepository = consultarRepository;
        }

        /// <summary>
        /// Executes the background service to process historic messages.
        /// </summary>
        /// <param name="stoppingToken">The cancellation token to stop the service.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string nombreMetodo = nameof(ExecuteAsync);
            try
            {
                this._logger.Inicio(TRACE_ID, nombreMetodo);
                while (!stoppingToken.IsCancellationRequested)
                {
                    int cantidadMensajesProcesados = await this._consultarRepository.ContarAsync<MensajeColaEntity>(TRACE_ID,
                                              mensaje => mensaje.EstadoId == (int)EstadoMensajeModel.Procesado ||

                                              mensaje.EstadoId == (int)EstadoMensajeModel.Cancelado);

                    if (cantidadMensajesProcesados > CANTIDAD_MENSAJES_MINIMA)
                    {
                        await this._guardarMensajesHistoricoService.MoverMensajesAHistoricoAsync(TRACE_ID);
                    }

                    int delayMinutos = cantidadMensajesProcesados switch
                    {
                            > UMBRAL_MUCHA_CARGA => DELAY_MUCHA_CARGA,    // Mucha carga: archivar cada minuto
                            > UMBRAL_CARGA_NORMAL => DELAY_CARGA_NORMAL,    // Carga normal: cada 2 minutos  
                            > UMBRAL_POCA_CARGA => DELAY_POCA_CARGA,     // Poca carga: cada 5 minutos
                            _ => DELAY_SIN_CARGA         // Sin carga: cada 10 minutos
                    };
                    await Task.Delay(TimeSpan.FromMinutes(delayMinutos), stoppingToken);
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