using System.Text.Json;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Services.ColasGrpcClientService;
using Japdeva.APIMovil.EnvioCorreos.Models;
using Japdeva.APIMovil.EnvioCorreos.Services.AgregarCorreoService;

namespace Japdeva.APIMovil.EnvioCorreos.BackgroundServices
{
    /// <summary>
    /// Servicio de fondo que consume la cola 'EnviarCorreo' y registra cada correo recibido para su envío.
    /// Mantiene un stream abierto con el microservicio de Colas y procesa cada mensaje en tiempo real.
    /// </summary>
    public class EnviarCorreoPorColaBackGroundService : BackgroundService
    {
        private readonly ILogger<EnviarCorreoPorColaBackGroundService> _logger;
        private readonly IColasGrpcClientService _colasGrpcClientService;
        private readonly IAgregarCorreoService _agregarCorreoService;
        private const int TIEMPO_ESPERA_RECONEXION = 5000;
        private const string TRACE_ID_BACKGROUND = "BACKGROUND_ENVIAR_CORREO_POR_COLA";
        private const string NOMBRE_COLA = "EnviarCorreo";
        private const string MENSAJE_CONTENIDO_INVALIDO = "El contenido del mensaje no pudo ser deserializado como solicitud de correo.";
        private const string MENSAJE_ACTUALIZAR_ESTADO = "Ocurrio un error al actualizar el estado del mensaje de la cola.";

        /// <summary>
        /// Inicializa una nueva instancia de EnviarCorreoPorColaBackGroundService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos del servicio de fondo.</param>
        /// <param name="colasGrpcClientService">Cliente gRPC del microservicio de Colas.</param>
        /// <param name="agregarCorreoService">Servicio para registrar correos en la cola de envío.</param>
        public EnviarCorreoPorColaBackGroundService(
            ILogger<EnviarCorreoPorColaBackGroundService> logger,
            IColasGrpcClientService colasGrpcClientService,
            IAgregarCorreoService agregarCorreoService)
        {
            this._logger = logger;
            this._colasGrpcClientService = colasGrpcClientService;
            this._agregarCorreoService = agregarCorreoService;
        }

        /// <summary>
        /// Ejecuta la lógica principal del servicio de fondo de forma continua.
        /// Mantiene el stream abierto y procesa cada mensaje conforme llega. Si el stream se interrumpe,
        /// espera <see cref="TIEMPO_ESPERA_RECONEXION"/> ms antes de reconectar.
        /// </summary>
        /// <param name="stoppingToken">Token de cancelación para detener el servicio de forma controlada.</param>
        /// <returns>Una tarea que representa la ejecución continua del servicio de fondo.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(TRACE_ID_BACKGROUND, nombreMetodo);
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await foreach (ColaMensajeModel cola in this._colasGrpcClientService.ObtenerMensajesPendientesAsync(TRACE_ID_BACKGROUND, NOMBRE_COLA, stoppingToken))
                        {
                            await this.ProcesarMensajeAsync(cola);
                        }
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        this._logger.Error(TRACE_ID_BACKGROUND, nombreMetodo, ex);
                        await Task.Delay(TIEMPO_ESPERA_RECONEXION, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.Error(TRACE_ID_BACKGROUND, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(TRACE_ID_BACKGROUND, nombreMetodo);
            }
        }

        /// <summary>
        /// Procesa un mensaje individual de la cola: deserializa el contenido y registra el correo para envío.
        /// </summary>
        /// <param name="cola">Mensaje recibido desde la cola.</param>
        public async Task ProcesarMensajeAsync(ColaMensajeModel cola)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            string traceIdMensaje = string.Empty;
            try
            {
                this._logger.Inicio(TRACE_ID_BACKGROUND, nombreMetodo);
                traceIdMensaje = cola.TraceId;

                var mensajeActualizado = await this._colasGrpcClientService.ActualizarMensajeEnProcesoAsync(TRACE_ID_BACKGROUND, cola.Id, traceIdMensaje);
                if (mensajeActualizado is null) throw new InvalidOperationException(MENSAJE_ACTUALIZAR_ESTADO);
                if (mensajeActualizado.TraceIdDiferente) return;
                traceIdMensaje = mensajeActualizado.TraceId;

                var solicitud = JsonSerializer.Deserialize<AgregarCorreoSolicitudModel>(cola.Contenido);
                if (solicitud is null) throw new InvalidOperationException(MENSAJE_CONTENIDO_INVALIDO);

                await this._agregarCorreoService.AgregarCorreoAsync(TRACE_ID_BACKGROUND, solicitud);
                await this._colasGrpcClientService.ActualizarMensajeExitosoAsync(TRACE_ID_BACKGROUND, cola.Id, traceIdMensaje);
            }
            catch (Exception ex)
            {
                await this._colasGrpcClientService.ActualizarMensajeFallidoAsync(TRACE_ID_BACKGROUND, cola.Id, traceIdMensaje);
                this._logger.Error(TRACE_ID_BACKGROUND, nombreMetodo, ex);
            }
            finally
            {
                this._logger.Fin(TRACE_ID_BACKGROUND, nombreMetodo);
            }
        }
    }
}
