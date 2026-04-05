using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Grpc.Core;
using Grpc.Net.Client;
using Japdeva.APIMovil.Common.ColasGrpc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Services.ColasGrpcClientMapperService;

namespace Japdeva.APIMovil.Common.Services.ColasGrpcClientService
{
    /// <summary>
    /// Implementación del cliente gRPC para el microservicio Japdeva.APIMovil.Colas.
    /// Gestiona el canal, la autenticación y el mapeo de tipos protobuf a modelos de dominio.
    /// El canal gRPC es reutilizable y thread-safe: se crea una sola vez al registrar el Singleton.
    /// </summary>
    public class ColasGrpcClientService : IColasGrpcClientService
    {
        private readonly ILogger<ColasGrpcClientService> _logger;
        private readonly ColaService.ColaServiceClient _cliente;
        private readonly Metadata _cabecerasAuth;
        private readonly IColasGrpcClientMapperService _mapper;

        private const string VARIABLE_COLAS_URL = "COLAS_URL";
        private const string VARIABLE_TOKEN_INTERNO = "GRPC_CLAVE_INTERNA";
        private const string CABECERA_TOKEN = "grpc-token";
        private const bool HABILITAR_MULTIPLES_CONEXIONES_HTTP2 = true;
        private const string MENSAJE_URL_NO_CONFIGURADA = "La variable de entorno 'COLAS_URL' no está configurada.";
        private const string MENSAJE_TOKEN_NO_CONFIGURADO = "La variable de entorno 'GRPC_CLAVE_INTERNA' no está configurada.";
        private const string MENSAJE_ACTUALIZACION_FALLIDA = "La actualización del mensaje en Colas retornó exito=false.";

        /// <summary>
        /// Inicializa el cliente gRPC creando el canal y las credenciales de autenticación.
        /// Lee las variables de entorno COLAS_URL y GRPC_CLAVE_INTERNA.
        /// </summary>
        /// <param name="logger">Logger para registro de operaciones.</param>
        /// <param name="mapper">Mapper para convertir respuestas protobuf al modelo de dominio.</param>
        public ColasGrpcClientService(ILogger<ColasGrpcClientService> logger, IColasGrpcClientMapperService mapper)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

            string url = Environment.GetEnvironmentVariable(VARIABLE_COLAS_URL)
                ?? throw new InvalidOperationException(MENSAJE_URL_NO_CONFIGURADA);
            string token = Environment.GetEnvironmentVariable(VARIABLE_TOKEN_INTERNO)
                ?? throw new InvalidOperationException(MENSAJE_TOKEN_NO_CONFIGURADO);

            SocketsHttpHandler httpHandler = new SocketsHttpHandler
            {
                EnableMultipleHttp2Connections = HABILITAR_MULTIPLES_CONEXIONES_HTTP2
            };
            GrpcChannel canal = GrpcChannel.ForAddress(url, new GrpcChannelOptions { HttpHandler = httpHandler });
            this._cliente = new ColaService.ColaServiceClient(canal);
            this._cabecerasAuth = new Metadata { { CABECERA_TOKEN, token } };
        }

        /// <summary>
        /// Publica un nuevo mensaje en la cola especificada.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="nombreCola">Nombre de la cola destino.</param>
        /// <param name="contenido">Contenido del mensaje en formato JSON.</param>
        /// <param name="idRpc">Identificador RPC para correlacionar la respuesta.</param>
        /// <param name="prioridad">Prioridad del mensaje: 1=Alta, 2=Media, 3=Baja.</param>
        /// <param name="metadatos">Metadatos adicionales del mensaje.</param>
        /// <returns>El mensaje publicado con su Id asignado por Colas.</returns>
        public async Task<ColaMensajeModel> PublicarMensajeAsync(string traceId, string nombreCola, string contenido, string idRpc, int prioridad, string metadatos = "")
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                PublicarMensajeRequest solicitud = new PublicarMensajeRequest
                {
                    NombreCola = nombreCola,
                    ContenidoMensaje = contenido,
                    IdRpc = idRpc,
                    Prioridad = prioridad,
                    Metadatos = metadatos,
                    TraceId = traceId
                };
                MensajeResponse respuesta = await this._cliente.PublicarMensajeAsync(solicitud, this._cabecerasAuth);
                return this._mapper.ConvertirMapAModelo(traceId, respuesta);
            }
            catch (Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(traceId, nombreMetodo);
            }
        }

        /// <summary>
        /// Abre un stream de mensajes pendientes para la cola indicada.
        /// El stream permanece abierto y entrega nuevos mensajes en tiempo real conforme llegan.
        /// Registra el inicio y fin del establecimiento del canal; la iteración real corre en el llamador.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="nombreCola">Nombre de la cola a consumir.</param>
        /// <param name="cancellationToken">Token para cerrar el stream.</param>
        /// <returns>Secuencia asíncrona de mensajes pendientes.</returns>
        public IAsyncEnumerable<ColaMensajeModel> ObtenerMensajesPendientesAsync(
            string traceId,
            string nombreCola,
            CancellationToken cancellationToken)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                ObtenerMensajesRequest solicitud = new ObtenerMensajesRequest { NombreCola = nombreCola };
                AsyncServerStreamingCall<MensajeResponse> llamada = this._cliente.ObtenerMensajesPendientes(
                    solicitud, this._cabecerasAuth, cancellationToken: cancellationToken);
                return IterarStreamLocal(traceId, llamada, cancellationToken);
            }
            catch (Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(traceId, nombreMetodo);
            }

            // Función local: no es miembro de clase, no aplican JAPDEVA055/081.
            // yield en try-finally (sin catch) es válido para CS1626.
            async IAsyncEnumerable<ColaMensajeModel> IterarStreamLocal(
                string traceIdLocal,
                AsyncServerStreamingCall<MensajeResponse> llamadaLocal,
                [EnumeratorCancellation] CancellationToken ct)
            {
                using AsyncServerStreamingCall<MensajeResponse> l = llamadaLocal;
                await foreach (MensajeResponse r in l.ResponseStream.ReadAllAsync(ct))
                    yield return this._mapper.ConvertirMapAModelo(traceIdLocal, r);
            }
        }

        /// <summary>
        /// Espera y retorna el mensaje identificado por IdRpc.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="nombreCola">Nombre de la cola.</param>
        /// <param name="idRpc">Identificador RPC del mensaje a esperar.</param>
        /// <param name="cancellationToken">Token de cancelación con timeout del llamador.</param>
        /// <returns>El mensaje cuando llegue, o null si se agota el tiempo de espera.</returns>
        public async Task<ColaMensajeModel?> ObtenerMensajePorIdRpcAsync(string traceId, string nombreCola, string idRpc, CancellationToken cancellationToken)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            ColaMensajeModel? resultado = null;
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                ObtenerPorIdRpcRequest solicitud = new ObtenerPorIdRpcRequest { IdRpc = idRpc, NombreCola = nombreCola };
                MensajeResponse respuesta = await this._cliente.ObtenerMensajePorIdRpcAsync(solicitud, this._cabecerasAuth, cancellationToken: cancellationToken);
                resultado = this._mapper.ConvertirMapAModelo(traceId, respuesta);
            }
            catch (RpcException rpcEx) when (rpcEx.StatusCode == StatusCode.NotFound)
            {
                // resultado permanece null — mensaje no encontrado en la cola
            }
            catch (OperationCanceledException)
            {
                // resultado permanece null — tiempo de espera agotado
            }
            catch (Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(traceId, nombreMetodo);
            }
            return resultado;
        }

        /// <summary>
        /// Notifica a Colas que el microservicio comenzó a procesar el mensaje.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Id del mensaje en Colas.</param>
        /// <param name="traceIdMensaje">TraceId original del mensaje.</param>
        /// <returns>El mensaje actualizado</returns>
        public async Task<ActualizarMensajeResponse> ActualizarMensajeEnProcesoAsync(string traceId, long id, string traceIdMensaje)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                ActualizarMensajeRequest solicitud = new ActualizarMensajeRequest { Id = id, TraceId = traceIdMensaje };
                ActualizarMensajeResponse respuesta = await this._cliente.ActualizarMensajeEnProcesoAsync(solicitud, this._cabecerasAuth);
                if (!respuesta.Exito) throw new InvalidOperationException(MENSAJE_ACTUALIZACION_FALLIDA);
                return respuesta;
            }
            catch (Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(traceId, nombreMetodo);
            }
        }

        /// <summary>
        /// Notifica a Colas que el mensaje fue procesado exitosamente.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Id del mensaje en Colas.</param>
        /// <param name="traceIdMensaje">TraceId original del mensaje.</param>
        public async Task ActualizarMensajeExitosoAsync(string traceId, long id, string traceIdMensaje)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                ActualizarMensajeRequest solicitud = new ActualizarMensajeRequest { Id = id, TraceId = traceIdMensaje };
                ActualizarMensajeResponse respuesta = await this._cliente.ActualizarMensajeExitosoAsync(solicitud, this._cabecerasAuth);
                if (!respuesta.Exito) throw new InvalidOperationException(MENSAJE_ACTUALIZACION_FALLIDA);
            }
            catch (Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(traceId, nombreMetodo);
            }
        }

        /// <summary>
        /// Notifica a Colas que el procesamiento del mensaje falló. El mensaje volverá a estado Pendiente.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Id del mensaje en Colas.</param>
        /// <param name="traceIdMensaje">TraceId original del mensaje.</param>
        public async Task ActualizarMensajeFallidoAsync(string traceId, long id, string traceIdMensaje)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                ActualizarMensajeRequest solicitud = new ActualizarMensajeRequest { Id = id, TraceId = traceIdMensaje };
                ActualizarMensajeResponse respuesta = await this._cliente.ActualizarMensajeFallidoAsync(solicitud, this._cabecerasAuth);
                if (!respuesta.Exito) throw new InvalidOperationException(MENSAJE_ACTUALIZACION_FALLIDA);
            }
            catch (Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(traceId, nombreMetodo);
            }
        }

    }
}
