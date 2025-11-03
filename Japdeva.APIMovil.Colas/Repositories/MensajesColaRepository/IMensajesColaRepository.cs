using Japdeva.APIMovil.Colas.Entities;

namespace Japdeva.APIMovil.Colas.Repositories.MensajesColaRepository
{
    /// <summary>
    /// Interfaz para el repositorio de operaciones de mensajes en colas.
    /// </summary>
    public interface IMensajesColaRepository
    {
        /// <summary>
        /// Obtiene los mensajes pendientes o fallidos de la cola de manera asíncrona.
        /// </summary>
        /// <param name="traceId">El identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <returns>Una lista de entidades de mensajes de cola pendientes o fallidos.</returns>
        Task<List<MensajeColaEntity>> ObtenerMensajesPendientesAsync(string traceId);
    }
}