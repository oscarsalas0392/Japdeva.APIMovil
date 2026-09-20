using Japdeva.APIMovil.Common.ColasGrpc;
using Japdeva.APIMovil.Common.Models;

namespace Japdeva.APIMovil.Common.Services.ColasGrpcClientService
{
    /// <summary>
    /// Cliente genérico para comunicación con el microservicio Japdeva.APIMovil.Colas via gRPC.
    /// Encapsula toda la complejidad del protocolo: canal, autenticación y mapeo de tipos.
    /// </summary>
    public interface IColasGrpcClientService
    {
        /// <summary>
        /// Publica un nuevo mensaje en la cola especificada.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="nombreCola">Nombre de la cola destino.</param>
        /// <param name="contenido">Contenido del mensaje en formato JSON.</param>
        /// <param name="idRpc">Identificador RPC para correlacionar la respuesta.</param>
        /// <param name="prioridad">Prioridad del mensaje: 1=Alta, 2=Media, 3=Baja.</param>
        /// <param name="metadatos">Metadatos adicionales del mensaje (opcional).</param>
        /// <returns>El mensaje publicado con su Id asignado por Colas.</returns>
        Task<ColaMensajeModel> PublicarMensajeAsync(string traceId, string nombreCola, string contenido, string idRpc, int prioridad, string metadatos = "");

        /// <summary>
        /// Abre un stream de mensajes pendientes para la cola indicada.
        /// El stream permanece abierto y entrega nuevos mensajes en tiempo real conforme llegan.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="nombreCola">Nombre de la cola a consumir.</param>
        /// <param name="cancellationToken">Token para cerrar el stream.</param>
        /// <returns>Secuencia asíncrona de mensajes pendientes.</returns>
        IAsyncEnumerable<ColaMensajeModel> ObtenerMensajesPendientesAsync(string traceId, string nombreCola, CancellationToken cancellationToken);

        /// <summary>
        /// Retorna el mensaje identificado por IdRpc aguardando su disponibilidad en Colas.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="nombreCola">Nombre de la cola.</param>
        /// <param name="idRpc">Identificador RPC del mensaje a esperar.</param>
        /// <param name="cancellationToken">Token de cancelación con timeout del llamador.</param>
        /// <returns>El mensaje cuando llegue, o null si se agota el tiempo de espera.</returns>
        Task<ColaMensajeModel?> ObtenerMensajePorIdRpcAsync(string traceId, string nombreCola, string idRpc, CancellationToken cancellationToken);

        /// <summary>
        /// Notifica a Colas que el microservicio comenzó a procesar el mensaje.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Id del mensaje en Colas.</param>
        /// <param name="traceIdMensaje">TraceId original del mensaje.</param>
        Task<ActualizarMensajeResponse> ActualizarMensajeEnProcesoAsync(string traceId, long id, string traceIdMensaje);

        /// <summary>
        /// Notifica a Colas que el mensaje fue procesado exitosamente.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Id del mensaje en Colas.</param>
        /// <param name="traceIdMensaje">TraceId original del mensaje.</param>
        Task ActualizarMensajeExitosoAsync(string traceId, long id, string traceIdMensaje);

        /// <summary>
        /// Notifica a Colas que el procesamiento del mensaje falló. El mensaje volverá a estado Pendiente.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Id del mensaje en Colas.</param>
        /// <param name="traceIdMensaje">TraceId original del mensaje.</param>
        Task ActualizarMensajeFallidoAsync(string traceId, long id, string traceIdMensaje);
    }
}
