using Microsoft.AspNetCore.Mvc;
using Grpc.Core;
using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Colas.Models;
using Japdeva.APIMovil.Colas.Protos;
using Japdeva.APIMovil.Colas.Services.ActualizarMensajeEnProcesoService;
using Japdeva.APIMovil.Colas.Services.ActualizarMensajeExitosoService;
using Japdeva.APIMovil.Colas.Services.ActualizarMensajeFallidoService;
using Japdeva.APIMovil.Colas.Services.EnviarMensajeService;
using Japdeva.APIMovil.Colas.Services.MensajeColaService;
using Japdeva.APIMovil.Colas.Services.ObtenerMensajePorIdRpcService;
using Japdeva.APIMovil.Colas.Services.SuscripcionColaService;
using Japdeva.APIMovil.Common.Extensions;

namespace Japdeva.APIMovil.Colas.Services.ColaGrpcService
{
    /// <summary>
    /// Implementación del servidor gRPC para gestión de colas de mensajes entre microservicios.
    /// </summary>
    public class ColaGrpcService : Protos.ColaService.ColaServiceBase
    {
        private readonly IEnviarMensajeService _enviarMensajeService;
        private readonly IMensajeColaService _mensajeColaService;
        private readonly ISuscripcionColaService _suscripcionColaService;
        private readonly IObtenerMensajePorIdRpcService _obtenerMensajePorIdRpcService;
        private readonly IActualizarMensajeEnProcesoService _actualizarMensajeEnProcesoService;
        private readonly IActualizarMensajeExitosoService _actualizarMensajeExitosoService;
        private readonly IActualizarMensajeFallidoService _actualizarMensajeFallidoService;
        private readonly ILogger<ColaGrpcService> _logger;
        private const bool EXITO = true;
        private const bool TRACE_ID_DIFERENTE_INICIAL = false;
        private const string MENSAJE_DATOS_INVALIDOS = "Datos del mensaje inválidos o cola no encontrada";
        private const string MENSAJE_ERROR_OBTENER_IDPRC = "Error al obtener mensaje por IdRpc";
        private const string MENSAJE_ERROR_EN_PROCESO = "No se pudo actualizar el mensaje a EnProceso";
        private const string MENSAJE_ERROR_EXITOSO = "No se pudo actualizar el mensaje a Exitoso";
        private const string MENSAJE_ERROR_FALLIDO = "No se pudo actualizar el mensaje a Fallido";
        private const string FORMATO_MENSAJE_NO_ENCONTRADO = "Mensaje con IdRpc '{0}' no encontrado en cola '{1}'";

        /// <summary>
        /// Inicializa una nueva instancia del servicio gRPC de colas.
        /// </summary>
        /// <param name="enviarMensajeService">Servicio para enviar mensajes a una cola.</param>
        /// <param name="mensajeColaService">Servicio para obtener mensajes del caché de la cola.</param>
        /// <param name="suscripcionColaService">Servicio para gestionar suscripciones push a colas.</param>
        /// <param name="obtenerMensajePorIdRpcService">Servicio para obtener un mensaje por su ID RPC.</param>
        /// <param name="actualizarMensajeEnProcesoService">Servicio para marcar mensajes en proceso.</param>
        /// <param name="actualizarMensajeExitosoService">Servicio para marcar mensajes como exitosos.</param>
        /// <param name="actualizarMensajeFallidoService">Servicio para marcar mensajes como fallidos.</param>
        /// <param name="logger">Logger para registro de eventos.</param>
        public ColaGrpcService(
            IEnviarMensajeService enviarMensajeService,
            IMensajeColaService mensajeColaService,
            ISuscripcionColaService suscripcionColaService,
            IObtenerMensajePorIdRpcService obtenerMensajePorIdRpcService,
            IActualizarMensajeEnProcesoService actualizarMensajeEnProcesoService,
            IActualizarMensajeExitosoService actualizarMensajeExitosoService,
            IActualizarMensajeFallidoService actualizarMensajeFallidoService,
            ILogger<ColaGrpcService> logger)
        {
            this._enviarMensajeService = enviarMensajeService
                ?? throw new ArgumentNullException(nameof(enviarMensajeService));
            this._mensajeColaService = mensajeColaService
                ?? throw new ArgumentNullException(nameof(mensajeColaService));
            this._suscripcionColaService = suscripcionColaService
                ?? throw new ArgumentNullException(nameof(suscripcionColaService));
            this._obtenerMensajePorIdRpcService = obtenerMensajePorIdRpcService
                ?? throw new ArgumentNullException(nameof(obtenerMensajePorIdRpcService));
            this._actualizarMensajeEnProcesoService = actualizarMensajeEnProcesoService
                ?? throw new ArgumentNullException(nameof(actualizarMensajeEnProcesoService));
            this._actualizarMensajeExitosoService = actualizarMensajeExitosoService
                ?? throw new ArgumentNullException(nameof(actualizarMensajeExitosoService));
            this._actualizarMensajeFallidoService = actualizarMensajeFallidoService
                ?? throw new ArgumentNullException(nameof(actualizarMensajeFallidoService));
            this._logger = logger
                ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Publica un nuevo mensaje en la cola especificada.
        /// </summary>
        /// <param name="request">Datos del mensaje a publicar.</param>
        /// <param name="context">Contexto de la llamada gRPC.</param>
        /// <returns>Datos del mensaje creado en la cola.</returns>
        public override async Task<MensajeResponse> PublicarMensaje(PublicarMensajeRequest request, ServerCallContext context)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            string traceId = context.GetHttpContext().TraceIdentifier;
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                var solicitud = new EnviarMensajeSolicitudModel
                {
                    IdRpc = request.IdRpc,
                    NombreCola = request.NombreCola,
                    ContenidoMensaje = request.ContenidoMensaje,
                    Prioridad = request.Prioridad,
                    TraceId = request.TraceId,
                    Metadatos = request.Metadatos
                };
                var result = await this._enviarMensajeService.EnviarMensajeAsync(traceId, solicitud);
                if (result is OkObjectResult ok && ok.Value is MensajeColasRespuestaModel modelo)
                    return MapToMensajeResponse(modelo);
                throw new RpcException(new Status(StatusCode.InvalidArgument, MENSAJE_DATOS_INVALIDOS));
            }
            catch (RpcException) { throw; }
            catch (Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
            finally { this._logger.Fin(traceId, nombreMetodo); }
        }

        /// <summary>
        /// Transmite los mensajes pendientes de una cola manteniendo el stream abierto para recibir nuevos mensajes en tiempo real.
        /// </summary>
        /// <param name="request">Nombre de la cola a suscribir.</param>
        /// <param name="responseStream">Stream de escritura para enviar mensajes al cliente.</param>
        /// <param name="context">Contexto de la llamada gRPC.</param>
        public override async Task ObtenerMensajesPendientes(ObtenerMensajesRequest request, IServerStreamWriter<MensajeResponse> responseStream, ServerCallContext context)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            string traceId = context.GetHttpContext().TraceIdentifier;
            var reader = this._suscripcionColaService.Suscribir(request.NombreCola);
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                var pendientes = await this._mensajeColaService.ObtenerMensajesNombreColaAsync(traceId, request.NombreCola);
                foreach (var entidad in pendientes)
                    await responseStream.WriteAsync(MapEntityToMensajeResponse(entidad));

                await foreach (var mensaje in reader.ReadAllAsync(context.CancellationToken))
                    await responseStream.WriteAsync(MapToMensajeResponse(mensaje));
            }
            catch (OperationCanceledException) { }
            catch (RpcException) { throw; }
            catch (Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
            finally
            {
                this._suscripcionColaService.Desuscribir(request.NombreCola, reader);
                this._logger.Fin(traceId, nombreMetodo);
            }
        }

        /// <summary>
        /// Obtiene un mensaje específico de la cola por su identificador RPC.
        /// </summary>
        /// <param name="request">Identificador RPC y nombre de la cola.</param>
        /// <param name="context">Contexto de la llamada gRPC.</param>
        /// <returns>Datos del mensaje encontrado.</returns>
        public override async Task<MensajeResponse> ObtenerMensajePorIdRpc(ObtenerPorIdRpcRequest request, ServerCallContext context)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            string traceId = context.GetHttpContext().TraceIdentifier;
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                var result = await this._obtenerMensajePorIdRpcService.ObtenerMensajeRpcAsync(traceId, request.IdRpc, request.NombreCola, context.CancellationToken);
                if (result is OkObjectResult ok && ok.Value is MensajeColasRespuestaModel modelo)
                    return MapToMensajeResponse(modelo);
                if (result is NotFoundObjectResult)
                    throw new RpcException(new Status(StatusCode.NotFound, string.Format(FORMATO_MENSAJE_NO_ENCONTRADO, request.IdRpc, request.NombreCola)));
                throw new RpcException(new Status(StatusCode.Internal, MENSAJE_ERROR_OBTENER_IDPRC));
            }
            catch (RpcException) { throw; }
            catch (Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
            finally { this._logger.Fin(traceId, nombreMetodo); }
        }

        /// <summary>
        /// Actualiza el estado de un mensaje a EnProceso.
        /// </summary>
        /// <param name="request">Identificador y TraceId del mensaje.</param>
        /// <param name="context">Contexto de la llamada gRPC.</param>
        /// <returns>Resultado de la actualización.</returns>
        public override async Task<ActualizarMensajeResponse> ActualizarMensajeEnProceso(ActualizarMensajeRequest request, ServerCallContext context) =>
            await EjecutarActualizacionAsync(context, request, this._actualizarMensajeEnProcesoService.ActualizarMensajeEnProcesoAsync, MENSAJE_ERROR_EN_PROCESO);

        /// <summary>
        /// Actualiza el estado de un mensaje a Exitoso.
        /// </summary>
        /// <param name="request">Identificador y TraceId del mensaje.</param>
        /// <param name="context">Contexto de la llamada gRPC.</param>
        /// <returns>Resultado de la actualización.</returns>
        public override async Task<ActualizarMensajeResponse> ActualizarMensajeExitoso(ActualizarMensajeRequest request, ServerCallContext context) =>
            await EjecutarActualizacionAsync(context, request, this._actualizarMensajeExitosoService.ActualizarMensajeExitosoAsync, MENSAJE_ERROR_EXITOSO);

        /// <summary>
        /// Actualiza el estado de un mensaje a Fallido e incrementa el contador de reintentos.
        /// </summary>
        /// <param name="request">Identificador y TraceId del mensaje.</param>
        /// <param name="context">Contexto de la llamada gRPC.</param>
        /// <returns>Resultado de la actualización.</returns>
        public override async Task<ActualizarMensajeResponse> ActualizarMensajeFallido(ActualizarMensajeRequest request, ServerCallContext context) =>
            await EjecutarActualizacionAsync(context, request, this._actualizarMensajeFallidoService.ActualizarMensajeFallidoAsync, MENSAJE_ERROR_FALLIDO);

        /// <summary>
        /// Ejecuta una operación de actualización de estado delegando al servicio correspondiente.
        /// </summary>
        /// <param name="context">Contexto de la llamada gRPC.</param>
        /// <param name="request">Datos de la solicitud de actualización.</param>
        /// <param name="servicio">Función del servicio a invocar.</param>
        /// <param name="mensajeError">Mensaje de error en caso de fallo.</param>
        /// <returns>Respuesta de la operación de actualización.</returns>
        public async Task<ActualizarMensajeResponse> EjecutarActualizacionAsync(ServerCallContext context, ActualizarMensajeRequest request, Func<string, ActualizarMensajeSolicitudModel, Task<IActionResult>> servicio, string mensajeError)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            string traceId = context.GetHttpContext().TraceIdentifier;
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                var solicitud = new ActualizarMensajeSolicitudModel { Id = request.Id, TraceId = request.TraceId };
                var result = await servicio(traceId, solicitud);
                if (result is OkObjectResult ok && ok.Value is MensajeColasRespuestaModel modelo)
                    return MapToActualizarResponse(modelo);
                throw new RpcException(new Status(StatusCode.InvalidArgument, mensajeError));
            }
            catch (RpcException) { throw; }
            catch (Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
            finally { this._logger.Fin(traceId, nombreMetodo); }
        }

        /// <summary>
        /// Convierte un modelo de respuesta de cola al mensaje gRPC correspondiente.
        /// </summary>
        /// <param name="modelo">Modelo de respuesta del dominio.</param>
        /// <returns>Mensaje gRPC mapeado.</returns>
        public static MensajeResponse MapToMensajeResponse(MensajeColasRespuestaModel modelo) =>
            new MensajeResponse
            {
                Id = modelo.Id,
                IdRpc = modelo.IdRpc,
                Cola = modelo.Cola,
                TraceId = modelo.TraceId,
                Mensaje = modelo.Mensaje,
                Estado = modelo.Estado,
                MetaDatos = modelo.MetaDatos,
                TraceIdDiferente = modelo.TraceIdDiferente
            };

        /// <summary>
        /// Convierte una entidad de mensaje de cola al mensaje gRPC correspondiente.
        /// </summary>
        /// <param name="entidad">Entidad de mensaje de cola.</param>
        /// <returns>Mensaje gRPC mapeado.</returns>
        public static MensajeResponse MapEntityToMensajeResponse(MensajeColaEntity entidad) =>
            new MensajeResponse
            {
                Id = entidad.Id,
                IdRpc = entidad.IdRpc ?? string.Empty,
                Cola = entidad.ColaId,
                TraceId = entidad.TraceId ?? string.Empty,
                Mensaje = entidad.ContenidoMensaje ?? string.Empty,
                Estado = entidad.EstadoId,
                MetaDatos = entidad.Metadatos ?? string.Empty,
                TraceIdDiferente = TRACE_ID_DIFERENTE_INICIAL
            };

        /// <summary>
        /// Convierte un modelo de respuesta de cola a la respuesta de actualización gRPC.
        /// </summary>
        /// <param name="modelo">Modelo de respuesta del dominio.</param>
        /// <returns>Respuesta de actualización gRPC mapeada.</returns>
        public static ActualizarMensajeResponse MapToActualizarResponse(MensajeColasRespuestaModel modelo) =>
            new ActualizarMensajeResponse { Exito = EXITO, TraceId = modelo.TraceId, TraceIdDiferente = modelo.TraceIdDiferente };
    }
}
