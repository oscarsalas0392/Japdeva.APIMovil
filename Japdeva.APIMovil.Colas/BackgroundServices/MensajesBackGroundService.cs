using Japdeva.APIMovil.Colas.Services.MensajeColaService;
using Japdeva.APIMovil.Common.Extensions;

namespace Japdeva.APIMovil.Colas.BackgroundServices
{
    /// <summary>
    /// Servicio en segundo plano para el procesamiento de mensajes en colas.
    /// </summary>
    public class MensajesBackgroundService : BackgroundService
    {
        private readonly ILogger<MensajesBackgroundService> _logger;
        private readonly IMensajeColaService _mensajeColaService;
        private const int DELAY_MILISEGUNDOS = 15;
        private const int DELAY_MILISEGUNDOS_MAXIMOS = 20;
        private const string TRACE_ID = "N/A";
        private const int CANTIDAD_MENSAJES_MINIMA = 0;

        /// <summary>
        /// Inicializa una nueva instancia de la clase MensajesBackgroundService.
        /// </summary>
        /// <param name="logger">Logger para registrar eventos del servicio.</param>
        /// <param name="mensajeColaService">Servicio para el manejo de mensajes en cola.</param>
        public MensajesBackgroundService(ILogger<MensajesBackgroundService> logger, IMensajeColaService mensajeColaService)
        {
            _logger = logger;
            _mensajeColaService = mensajeColaService;
        }

        /// <summary>
        /// Ejecuta el procesamiento de mensajes en segundo plano de forma asíncrona.
        /// </summary>
        /// <param name="stoppingToken">Token de cancelación para detener el servicio.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            this._logger.Inicio(TRACE_ID, nombreMetodo);
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        int cantidadMensajesPendientes = await this._mensajeColaService.ContarMensajesPendientesAsync(TRACE_ID);

                        if (cantidadMensajesPendientes > CANTIDAD_MENSAJES_MINIMA)
                        {
                            await this._mensajeColaService.LlenarCacheMensajesAsync(TRACE_ID);
                            await Task.Delay(TimeSpan.FromMilliseconds(DELAY_MILISEGUNDOS), stoppingToken);
                        }
                        else
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(DELAY_MILISEGUNDOS_MAXIMOS), stoppingToken);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        this._logger.Error(TRACE_ID, nombreMetodo, ex);
                        await Task.Delay(TimeSpan.FromMilliseconds(DELAY_MILISEGUNDOS_MAXIMOS), stoppingToken);
                    }
                }
            }
            finally
            {
                this._logger.Fin(TRACE_ID, nombreMetodo);
            }
        }
    }
}