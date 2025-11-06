
using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Colas.Models;
using Japdeva.APIMovil.Colas.Services.ActualizarMensajeExpiradoService;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;


namespace Japdeva.APIMovil.Colas.BackgroundServices
{
    /// <summary>
    /// Servicio en segundo plano que se encarga de procesar y actualizar mensajes de cola expirados.
    /// </summary>
    public class MensajeColaExpiradosService : BackgroundService
    {
        private readonly ILogger<MensajeColaExpiradosService> _logger;
        private readonly IActualizarMensajeExpiradoService _actualizarMensajeExpiradoService;
        private readonly IServiceProvider _serviceProvider;
        private const int DELAY_MINUTOS = 5;
        private const string TRACE_ID = "N/A";
        private const int CANTIDAD_MENSAJES_MINIMA = 0;
        private const int DIAS_EXPIRACION = -5;
        private const int NUMERO_PAGINA = 1;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="MensajeColaExpiradosService"/>.
        /// </summary>
        /// <param name="logger">Logger para registrar eventos y errores.</param>
        /// <param name="actualizarMensajeExpiradoService">Servicio para actualizar mensajes expirados.</param>
        /// <param name="serviceProvider">Proveedor de servicios para la inyección de dependencias.</param>
        public MensajeColaExpiradosService(
            ILogger<MensajeColaExpiradosService> logger,
            IActualizarMensajeExpiradoService actualizarMensajeExpiradoService,
            IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._actualizarMensajeExpiradoService = actualizarMensajeExpiradoService;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Ejecuta el servicio en segundo plano para procesar mensajes de cola expirados.
        /// </summary>
        /// <param name="stoppingToken">Token de cancelación para detener la ejecución del servicio.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(TRACE_ID, nombreMetodo);
                while (!stoppingToken.IsCancellationRequested)
                {
                    DateTime fechaLimite = DateTime.UtcNow.AddDays(DIAS_EXPIRACION);
                    int cantidadMensajesExpirados = await this.ContarMensajesExpiradosAsync(fechaLimite);

                    if (cantidadMensajesExpirados > CANTIDAD_MENSAJES_MINIMA)
                    {
                        using var scope = this._serviceProvider.CreateScope();
                        var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                        var listaMensajes = await consultarListaRepository.ConsultarListaAsync<MensajeColaEntity>(TRACE_ID, NUMERO_PAGINA, mensaje =>
                             (mensaje.EstadoId == (int)EstadoMensajeModel.Pendiente ||
                             mensaje.EstadoId == (int)EstadoMensajeModel.EnProceso ||
                             mensaje.EstadoId == (int)EstadoMensajeModel.Fallido) &&
                            (mensaje.FechaRegistro <= fechaLimite));

                        if (listaMensajes is not null && listaMensajes.Lista.Any())
                        {
                            foreach (var mensaje in listaMensajes.Lista)
                            {
                                await this._actualizarMensajeExpiradoService.ActualizarMensajeExpiradoAsync(TRACE_ID, mensaje.Id, mensaje.TraceId);
                            }
                        }
                    }
                    await Task.Delay(TimeSpan.FromMinutes(DELAY_MINUTOS), stoppingToken);
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
        /// Cuenta la cantidad de mensajes de cola que han expirado según los criterios establecidos.
        /// </summary>
        /// <param name="fechaLimite">La fecha límite para considerar un mensaje como expirado.</param>
        /// <returns>El número total de mensajes expirados.</returns>
        public async Task<int> ContarMensajesExpiradosAsync(DateTime fechaLimite)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(TRACE_ID, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                int totalMensajesExpirados = await consultarRepository.ContarAsync<MensajeColaEntity>(TRACE_ID, mensaje =>
                    (mensaje.EstadoId == (int)EstadoMensajeModel.Pendiente ||
                    mensaje.EstadoId == (int)EstadoMensajeModel.EnProceso ||
                    mensaje.EstadoId == (int)EstadoMensajeModel.Fallido) &&
                    (mensaje.FechaRegistro <= fechaLimite)); 

                return totalMensajesExpirados;

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