using Japdeva.APIMovil.Common.Models;

namespace Japdeva.APIMovil.Common.Services.ColaRpcService
{
    /// <summary>
    /// Contrato para operaciones RPC de alto nivel sobre el sistema de colas.
    /// Orquesta publicación y espera de respuesta en una sola llamada.
    /// </summary>
    public interface IColaRpcService
    {
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
        Task<ColaMensajeModel?> EnviarYEsperarRespuestaAsync(string traceId, string nombreCola, string nombreColaRespuesta, string contenido, CancellationToken cancellationToken);
    }
}
