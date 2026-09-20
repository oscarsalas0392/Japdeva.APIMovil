using Grpc.Core;
using Grpc.Core.Interceptors;
using Japdeva.APIMovil.Common.Extensions;

namespace Japdeva.APIMovil.Colas.Interceptors
{
    /// <summary>
    /// Interceptor gRPC para validar el token de autenticación entre microservicios.
    /// </summary>
    public class TokenGrpcInterceptor : Interceptor
    {
        private readonly ILogger<TokenGrpcInterceptor> _logger;
        private const string CABECERA_TOKEN = "grpc-token";
        private const string ENV_CLAVE_INTERNA = "GRPC_CLAVE_INTERNA";
        private const string MENSAJE_NO_AUTORIZADO = "Token inter-servicio inválido o no proporcionado.";

        /// <summary>
        /// Inicializa una nueva instancia de TokenGrpcInterceptor.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos de autenticación.</param>
        public TokenGrpcInterceptor(ILogger<TokenGrpcInterceptor> logger)
        {
            this._logger = logger;
        }

        /// <summary>
        /// Intercepta llamadas unarias del servidor para validar el token.
        /// </summary>
        /// <typeparam name="TRequest">Tipo del mensaje de solicitud.</typeparam>
        /// <typeparam name="TResponse">Tipo del mensaje de respuesta.</typeparam>
        /// <param name="request">Solicitud entrante.</param>
        /// <param name="context">Contexto de la llamada gRPC.</param>
        /// <param name="continuation">Delegado que continúa el procesamiento.</param>
        /// <returns>Respuesta del servidor.</returns>
        public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
            TRequest request,
            ServerCallContext context,
            UnaryServerMethod<TRequest, TResponse> continuation)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(context.GetHttpContext().TraceIdentifier, nombreMetodo);
                ValidarToken(context);
                return await continuation(request, context);
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                this._logger.Error(context.GetHttpContext().TraceIdentifier, nombreMetodo, ex);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
            finally
            {
                this._logger.Fin(context.GetHttpContext().TraceIdentifier, nombreMetodo);
            }
        }

        /// <summary>
        /// Intercepta llamadas de streaming del servidor para validar el token.
        /// </summary>
        /// <typeparam name="TRequest">Tipo del mensaje de solicitud.</typeparam>
        /// <typeparam name="TResponse">Tipo del mensaje de respuesta.</typeparam>
        /// <param name="request">Solicitud entrante.</param>
        /// <param name="responseStream">Stream de respuesta hacia el cliente.</param>
        /// <param name="context">Contexto de la llamada gRPC.</param>
        /// <param name="continuation">Delegado que continúa el procesamiento.</param>
        public override async Task ServerStreamingServerHandler<TRequest, TResponse>(
            TRequest request,
            IServerStreamWriter<TResponse> responseStream,
            ServerCallContext context,
            ServerStreamingServerMethod<TRequest, TResponse> continuation)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(context.GetHttpContext().TraceIdentifier, nombreMetodo);
                ValidarToken(context);
                await continuation(request, responseStream, context);
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                this._logger.Error(context.GetHttpContext().TraceIdentifier, nombreMetodo, ex);
                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
            }
            finally
            {
                this._logger.Fin(context.GetHttpContext().TraceIdentifier, nombreMetodo);
            }
        }

        /// <summary>
        /// Valida el token de autenticación inter-servicio en el contexto de la llamada gRPC.
        /// </summary>
        /// <param name="context">Contexto de la llamada gRPC con las cabeceras de la solicitud.</param>
        public void ValidarToken(ServerCallContext context)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(context.GetHttpContext().TraceIdentifier, nombreMetodo);
                string? claveEsperada = Environment.GetEnvironmentVariable(ENV_CLAVE_INTERNA);
                if (string.IsNullOrEmpty(claveEsperada)) return;
                string? token = context.RequestHeaders.GetValue(CABECERA_TOKEN);
                if (token != claveEsperada)
                    throw new RpcException(new Status(StatusCode.Unauthenticated, MENSAJE_NO_AUTORIZADO));
            }
            catch (RpcException)
            {
                throw;
            }
            catch (Exception ex)
            {
                this._logger.Error(context.GetHttpContext().TraceIdentifier, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(context.GetHttpContext().TraceIdentifier, nombreMetodo);
            }
        }
    }
}
