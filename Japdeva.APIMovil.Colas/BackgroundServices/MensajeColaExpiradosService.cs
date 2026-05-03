using Japdeva.APIMovil.Colas.Repositories.MensajesColaRepository;
using Japdeva.APIMovil.Common.Extensions;

namespace Japdeva.APIMovil.Colas.BackgroundServices
{
    /// <summary>
    /// Servicio en segundo plano que marca como expirados en bloque los mensajes de cola.
    /// Maneja dos tipos de expiración: mensajes RPC de alta prioridad (3 minutos) y mensajes
    /// generales (5 días). Ambas expiraciones se ejecutan en paralelo mediante una única
    /// operación SQL por tipo. El intervalo entre ciclos se adapta según la carga detectada.
    /// </summary>
    public class MensajeColaExpiradosService : BackgroundService
    {
        private readonly ILogger<MensajeColaExpiradosService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const string TRACE_ID = "N/A";
        private const int DIAS_EXPIRACION_GENERAL = -5;
        private const int MINUTOS_EXPIRACION_RPC = -3;
        private const int UMBRAL_MUCHA_CARGA = 500;
        private const int UMBRAL_CARGA_NORMAL = 100;
        private const int UMBRAL_POCA_CARGA = 10;
        private const int DELAY_MUCHA_CARGA = 1;
        private const int DELAY_CARGA_NORMAL = 3;
        private const int DELAY_POCA_CARGA = 5;
        private const int DELAY_SIN_CARGA = 10;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="MensajeColaExpiradosService"/>.
        /// </summary>
        /// <param name="logger">Logger para registrar eventos y errores.</param>
        /// <param name="serviceProvider">Proveedor de servicios para la inyección de dependencias.</param>
        public MensajeColaExpiradosService(
            ILogger<MensajeColaExpiradosService> logger,
            IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Ejecuta el ciclo de expiración masiva de mensajes. En cada iteración lanza en paralelo
        /// la expiración de mensajes RPC (3 minutos) y la expiración general (5 días).
        /// El delay entre ciclos se adapta según el total de mensajes expirados.
        /// </summary>
        /// <param name="stoppingToken">Token de cancelación para detener la ejecución del servicio.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(TRACE_ID, nombreMetodo);
                while (!stoppingToken.IsCancellationRequested)
                {
                    Task<int> tareaRpc = this.ExpirarMensajesRpcAsync();
                    Task<int> tareaGeneral = this.ExpirarMensajesGeneralesAsync();
                    await Task.WhenAll(tareaRpc, tareaGeneral);

                    int totalExpirados = tareaRpc.Result + tareaGeneral.Result;

                    int delayMinutos = totalExpirados switch
                    {
                        > UMBRAL_MUCHA_CARGA   => DELAY_MUCHA_CARGA,
                        > UMBRAL_CARGA_NORMAL  => DELAY_CARGA_NORMAL,
                        > UMBRAL_POCA_CARGA    => DELAY_POCA_CARGA,
                        _                      => DELAY_SIN_CARGA
                    };

                    await Task.Delay(TimeSpan.FromMinutes(delayMinutos), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                this._logger.Error(TRACE_ID, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(TRACE_ID, nombreMetodo);
            }
        }

        /// <summary>
        /// Expira en bloque los mensajes generales cuya antigüedad supera los 5 días.
        /// </summary>
        /// <returns>Cantidad de mensajes generales marcados como expirados.</returns>
        public async Task<int> ExpirarMensajesGeneralesAsync()
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(TRACE_ID, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IMensajesColaRepository>();
                DateTime fechaLimite = DateTime.UtcNow.AddDays(DIAS_EXPIRACION_GENERAL);
                return await repo.ExpirarMensajesAsync(TRACE_ID, fechaLimite);
            }
            catch (Exception ex)
            {
                this._logger.Error(TRACE_ID, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(TRACE_ID, nombreMetodo);
            }
        }

        /// <summary>
        /// Expira en bloque los mensajes RPC de alta prioridad con IdRpc no vacío
        /// cuya antigüedad supera los 3 minutos.
        /// </summary>
        /// <returns>Cantidad de mensajes RPC marcados como expirados.</returns>
        public async Task<int> ExpirarMensajesRpcAsync()
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(TRACE_ID, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IMensajesColaRepository>();
                DateTime fechaLimite = DateTime.UtcNow.AddMinutes(MINUTOS_EXPIRACION_RPC);
                return await repo.ExpirarMensajesRpcAsync(TRACE_ID, fechaLimite);
            }
            catch (Exception ex)
            {
                this._logger.Error(TRACE_ID, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(TRACE_ID, nombreMetodo);
            }
        }
    }
}
