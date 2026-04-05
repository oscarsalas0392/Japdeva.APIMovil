using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Services.ColasGrpcClientService;

namespace Japdeva.APIMovil.Common.Services.ColaRpcService
{
    /// <summary>
    /// Servicio de alto nivel para operaciones RPC sobre el sistema de colas.
    /// Orquesta la publicación de una solicitud y la espera de su respuesta correlacionada por IdRpc.
    /// </summary>
    public class ColaRpcService : IColaRpcService
    {
        private readonly ILogger<ColaRpcService> _logger;
        private readonly IColasGrpcClientService _colasGrpcClientService;

        private const int PRIORIDAD_ALTA = 1;
        private const string FORMATO_METADATOS_COLA_RESPUESTA = "{{\"colaRespuesta\":\"{0}\"}}";

        /// <summary>
        /// Inicializa el servicio con el cliente gRPC de Colas.
        /// </summary>
        /// <param name="logger">Logger para registro de operaciones.</param>
        /// <param name="colasGrpcClientService">Cliente gRPC del microservicio Colas.</param>
        public ColaRpcService(ILogger<ColaRpcService> logger, IColasGrpcClientService colasGrpcClientService)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._colasGrpcClientService = colasGrpcClientService ?? throw new ArgumentNullException(nameof(colasGrpcClientService));
        }

        /// <summary>
        /// Publica un mensaje en la cola de solicitud y aguarda la respuesta en la cola de respuesta.
        /// Genera un IdRpc único para correlacionar la solicitud con su respuesta.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="nombreCola">Nombre de la cola donde se publica la solicitud.</param>
        /// <param name="nombreColaRespuesta">Nombre de la cola donde se espera la respuesta.</param>
        /// <param name="contenido">Contenido del mensaje en formato JSON.</param>
        /// <param name="cancellationToken">Token de cancelación con timeout del llamador.</param>
        /// <returns>El mensaje de respuesta, o null si se agota el tiempo de espera.</returns>
        public async Task<ColaMensajeModel?> EnviarYEsperarRespuestaAsync(string traceId, string nombreCola, string nombreColaRespuesta, string contenido, CancellationToken cancellationToken)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            ColaMensajeModel? resultado = null;
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                string idRpc = Guid.NewGuid().ToString();
                string metadatos = string.Format(FORMATO_METADATOS_COLA_RESPUESTA, nombreColaRespuesta);
                await this._colasGrpcClientService.PublicarMensajeAsync(traceId, nombreCola, contenido, idRpc, PRIORIDAD_ALTA, metadatos);
                resultado = await this._colasGrpcClientService.ObtenerMensajePorIdRpcAsync(traceId, nombreColaRespuesta, idRpc, cancellationToken);
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
    }
}
