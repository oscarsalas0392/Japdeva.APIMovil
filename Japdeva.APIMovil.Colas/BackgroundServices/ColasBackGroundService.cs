using Japdeva.APIMovil.Colas.Services.ColaService;
using Japdeva.APIMovil.Common.Extensions;

namespace Japdeva.APIMovil.Colas.BackgroundServices
{
    /// <summary>
    /// Servicio en segundo plano para el procesamiento de mensajes en colas.
    /// </summary>
    public class ColasBackgroundService : BackgroundService
    {
        private readonly ILogger<ColasBackgroundService> _logger;
        private readonly IColaService _colaService;
        private int _cantidadColas = 0;
        private DateTime _ultimoRefrescoPeriodico = DateTime.MinValue;
        private const int DELAY_MILISEGUNDOS = 500;
        private const int DELAY_SEGUNDOS = 5;
        private const int INTERVALO_REFRESCO_MINUTOS = 5;
        private const string TRACE_ID = "N/A";

        /// <summary>
        /// Inicializa una nueva instancia de la clase MensajesBackgroundService.
        /// </summary>
        /// <param name="logger">Logger para registrar eventos del servicio.</param>
        /// <param name="mensajeColaService">Servicio para el manejo de mensajes en cola.</param>
        public ColasBackgroundService(ILogger<ColasBackgroundService> logger, IColaService mensajeColaService)
        {
            _logger = logger;
            _colaService = mensajeColaService;
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
                        int cantidadColas = await this._colaService.ContarColasActivasAsync(TRACE_ID);
                        bool contaoCambio = cantidadColas != this._cantidadColas;
                        bool refrescoPeriodico = (DateTime.UtcNow - this._ultimoRefrescoPeriodico) >= TimeSpan.FromMinutes(INTERVALO_REFRESCO_MINUTOS);

                        if (contaoCambio || refrescoPeriodico)
                        {
                            this._cantidadColas = cantidadColas;
                            this._ultimoRefrescoPeriodico = DateTime.UtcNow;
                            await this._colaService.LlenarCacheColasAsync(TRACE_ID);
                            await Task.Delay(TimeSpan.FromMilliseconds(DELAY_MILISEGUNDOS), stoppingToken);
                        }
                        else
                        {
                            await Task.Delay(TimeSpan.FromSeconds(DELAY_SEGUNDOS), stoppingToken);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        this._logger.Error(TRACE_ID, nombreMetodo, ex);
                        await Task.Delay(TimeSpan.FromSeconds(DELAY_SEGUNDOS), stoppingToken);
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