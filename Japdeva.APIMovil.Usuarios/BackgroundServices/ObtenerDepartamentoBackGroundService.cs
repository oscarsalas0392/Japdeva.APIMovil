using System.Text.Json;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Services.ColasGrpcClientService;
using Japdeva.APIMovil.Usuarios.Models;
using Japdeva.APIMovil.Usuarios.Services.DepartamentoCacheService;

namespace Japdeva.APIMovil.Usuarios.BackgroundServices
{
    /// <summary>
    /// Servicio de fondo que procesa mensajes de la cola ObtenerDepartamento.
    /// Recibe el ID del departamento y responde con su descripción.
    /// </summary>
    public class ObtenerDepartamentoBackGroundService : BackgroundService
    {
        private readonly ILogger<ObtenerDepartamentoBackGroundService> _logger;
        private readonly IColasGrpcClientService _colasGrpcClientService;
        private readonly IDepartamentoCacheService _departamentoCacheService;
        private const int TIEMPO_ESPERA_RECONEXION = 30;
        private const int PRIORIDAD_ALTA = 1;
        private const string TRACE_ID_BACKGROUND = "BACKGROUND_OBTENER_DEPARTAMENTO";
        private const string NOMBRE_COLA = "ObtenerDepartamento";
        private const string CLAVE_COLA_RESPUESTA = "colaRespuesta";
        private const string MENSAJE_COLA_RESPUESTA_VACIA = "No se pudo obtener el nombre de la cola de respuesta desde los metadatos.";
        private const string MENSAJE_DEPARTAMENTO_NO_ENCONTRADO = "Departamento no encontrado para el ID recibido.";
        private const string MENSAJE_ERROR_ACTUALIZAR_ESTADO = "Ocurrio un error al actualizar el estado del mensaje de la cola.";

        /// <summary>
        /// Inicializa una nueva instancia del servicio de fondo para obtención de datos de departamento.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="colasGrpcClientService">Cliente gRPC del microservicio de Colas.</param>
        /// <param name="departamentoCacheService">Servicio de caché de departamentos.</param>
        public ObtenerDepartamentoBackGroundService(
            ILogger<ObtenerDepartamentoBackGroundService> logger,
            IColasGrpcClientService colasGrpcClientService,
            IDepartamentoCacheService departamentoCacheService)
        {
            this._logger = logger;
            this._colasGrpcClientService = colasGrpcClientService;
            this._departamentoCacheService = departamentoCacheService;
        }

        /// <summary>
        /// Ejecuta el loop principal que mantiene el stream abierto con Colas y procesa mensajes en tiempo real.
        /// </summary>
        /// <param name="stoppingToken">Token para detener el servicio de forma controlada.</param>
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
        /// Procesa un mensaje de la cola: busca el departamento por ID en caché y publica la respuesta.
        /// </summary>
        /// <param name="cola">Mensaje recibido desde la cola ObtenerDepartamento.</param>
        public async Task ProcesarMensajeAsync(ColaMensajeModel cola)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            string traceIdMensaje = string.Empty;
            try
            {
                this._logger.Inicio(TRACE_ID_BACKGROUND, nombreMetodo);
                traceIdMensaje = cola.TraceId;

                var mensajeActualizado = await this._colasGrpcClientService.ActualizarMensajeEnProcesoAsync(TRACE_ID_BACKGROUND, cola.Id, traceIdMensaje);
                if (mensajeActualizado is null) throw new Exception(MENSAJE_ERROR_ACTUALIZAR_ESTADO);
                if (mensajeActualizado.TraceIdDiferente) return;
                traceIdMensaje = mensajeActualizado.TraceId;

                int idDepartamento = Convert.ToInt32(cola.Contenido);
                var departamento = this._departamentoCacheService.ObtenerPorId(traceIdMensaje, idDepartamento);
                if (departamento is null) throw new KeyNotFoundException(MENSAJE_DEPARTAMENTO_NO_ENCONTRADO);

                var metaDatos = JsonSerializer.Deserialize<Dictionary<string, string>>(cola.MetaDatos);
                if (metaDatos is null || !metaDatos.TryGetValue(CLAVE_COLA_RESPUESTA, out string? colaRespuesta) || string.IsNullOrEmpty(colaRespuesta))
                    throw new InvalidOperationException(MENSAJE_COLA_RESPUESTA_VACIA);

                DepartamentoRespuestaModel respuesta = new DepartamentoRespuestaModel
                {
                    Id = departamento.Id,
                    Descripcion = departamento.Descripcion,
                    Activo = departamento.Activo
                };

                string json = JsonSerializer.Serialize(respuesta);
                Task publicar = this._colasGrpcClientService.PublicarMensajeAsync(TRACE_ID_BACKGROUND, colaRespuesta, json, cola.IdRpc, PRIORIDAD_ALTA, string.Empty);
                Task actualizar = this._colasGrpcClientService.ActualizarMensajeExitosoAsync(TRACE_ID_BACKGROUND, cola.Id, traceIdMensaje);
                await Task.WhenAll(publicar, actualizar);
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
