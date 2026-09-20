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
        private readonly IServiceProvider _serviceProvider;
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
        /// <param name="serviceProvider">The service provider for dependency injection.</param>
        public MensajesHistoricoBackgroundService(ILogger<MensajesHistoricoBackgroundService> logger, IGuardarMensajesHistoricoService guardarMensajesHistoricoService, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _guardarMensajesHistoricoService = guardarMensajesHistoricoService;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Executes the background service to process historic messages.
        /// </summary>
        /// <param name="stoppingToken">The cancellation token to stop the service.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string nombreMetodo = nameof(ExecuteAsync);
            this._logger.Inicio(TRACE_ID, nombreMetodo);
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    int cantidadMensajesProcesados = CANTIDAD_MENSAJES_MINIMA;
                    try
                    {
                        using var scope = this._serviceProvider.CreateScope();
                        var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                        cantidadMensajesProcesados = await consultarRepository.ContarAsync<MensajeColaEntity>(TRACE_ID,
                            mensaje => mensaje.EstadoId == (int)EstadoMensajeModel.Procesado ||
                                       mensaje.EstadoId == (int)EstadoMensajeModel.Cancelado ||
                                       mensaje.EstadoId == (int)EstadoMensajeModel.Expirado);

                        if (cantidadMensajesProcesados > CANTIDAD_MENSAJES_MINIMA)
                            await this._guardarMensajesHistoricoService.MoverMensajesAHistoricoAsync(TRACE_ID);
                    }
                    catch (Exception ex)
                    {
                        this._logger.Error(TRACE_ID, nombreMetodo, ex);
                    }

                    int delayMinutos = cantidadMensajesProcesados switch
                    {
                        > UMBRAL_MUCHA_CARGA => DELAY_MUCHA_CARGA,
                        > UMBRAL_CARGA_NORMAL => DELAY_CARGA_NORMAL,
                        > UMBRAL_POCA_CARGA => DELAY_POCA_CARGA,
                        _ => DELAY_SIN_CARGA
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