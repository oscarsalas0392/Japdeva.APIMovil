using Newtonsoft.Json;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Services.ColasGrpcClientService;
using Japdeva.APIMovil.Parametros.Models;
using Japdeva.APIMovil.Parametros.Services.PlantillaCorreoCacheService;

namespace Japdeva.APIMovil.Parametros.BackGroundServices
{
    /// <summary>
    /// Servicio de fondo que procesa mensajes de la cola ObtenerPlantilla.
    /// Mantiene un stream abierto con el microservicio de Colas y procesa cada mensaje en tiempo real.
    /// </summary>
    public class ObtenerPlantillaBackGroundService : BackgroundService
    {
        private readonly ILogger<ObtenerPlantillaBackGroundService> _logger;
        private readonly IColasGrpcClientService _colasGrpcClientService;
        private readonly IPlantillaCorreoCacheService _plantillaCorreoCacheService;
        private const int TIEMPO_ESPERA_RECONEXION = 30;
        private const int PRIORIDAD_ALTA = 1;
        private const string TRACE_ID_BACKGROUND = "BACKGROUND_OBTENER_PLANTILLA";
        private const string NOMBRE_COLA = "ObtenerPlantilla";
        private const string CLAVE_COLA_RESPUESTA = "colaRespuesta";
        private const string MENSAJE_COLA_RESPUESTA_VACIA = "No se pudo obtener el nombre de la cola de respuesta desde los metadatos.";
        private const string MENSAJE_PLANTILLA_NO_ENCONTRADA = "Plantilla de correo no encontrada en caché.";
        private const string MENSAJE_ACTUALIZAR_ESTADO = "Ocurrio un error al actualizar el estado del mensaje de la cola.";

        /// <summary>
        /// Inicializa una nueva instancia del servicio de fondo para obtención de plantillas.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos del servicio de fondo.</param>
        /// <param name="colasGrpcClientService">Cliente gRPC del microservicio de Colas.</param>
        /// <param name="plantillaCorreoCacheService">Servicio de caché de plantillas de correo.</param>
        public ObtenerPlantillaBackGroundService(
            ILogger<ObtenerPlantillaBackGroundService> logger,
            IColasGrpcClientService colasGrpcClientService,
            IPlantillaCorreoCacheService plantillaCorreoCacheService)
        {
            this._logger = logger;
            this._colasGrpcClientService = colasGrpcClientService;
            this._plantillaCorreoCacheService = plantillaCorreoCacheService;
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
                   
                    }

                    await Task.Delay(TIEMPO_ESPERA_RECONEXION, stoppingToken);
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
        /// Procesa un mensaje individual de la cola: obtiene la plantilla del caché y publica la respuesta.
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
                if (mensajeActualizado is null) throw new Exception(MENSAJE_ACTUALIZAR_ESTADO);
                if (mensajeActualizado.TraceIdDiferente) return;
                traceIdMensaje = mensajeActualizado.TraceId;

                int idPlantilla = Convert.ToInt32(cola.Contenido);
                var plantilla = this._plantillaCorreoCacheService.ObtenerPlantillaCorreoPorId(TRACE_ID_BACKGROUND, idPlantilla);
                if (plantilla is null) throw new KeyNotFoundException(MENSAJE_PLANTILLA_NO_ENCONTRADA);

                var metaDatos = JsonConvert.DeserializeObject<Dictionary<string, string>>(cola.MetaDatos);
                string? colaRespuesta = string.Empty;
                if (metaDatos is null || !metaDatos.TryGetValue(CLAVE_COLA_RESPUESTA, out colaRespuesta) || string.IsNullOrEmpty(colaRespuesta)) throw new InvalidOperationException(MENSAJE_COLA_RESPUESTA_VACIA);
                
                PlantillaRespuestaModel plantillaRespuestaModel = new PlantillaRespuestaModel();
                plantillaRespuestaModel.Plantilla = plantilla.Plantilla;
                string json = JsonConvert.SerializeObject(plantillaRespuestaModel);
                Task publicar = this._colasGrpcClientService.PublicarMensajeAsync(TRACE_ID_BACKGROUND, colaRespuesta, json, cola.IdRpc, PRIORIDAD_ALTA, string.Empty);
                Task actualizarMensajeExitoso = this._colasGrpcClientService.ActualizarMensajeExitosoAsync(TRACE_ID_BACKGROUND, cola.Id, traceIdMensaje);
                await Task.WhenAll(publicar, actualizarMensajeExitoso);
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
