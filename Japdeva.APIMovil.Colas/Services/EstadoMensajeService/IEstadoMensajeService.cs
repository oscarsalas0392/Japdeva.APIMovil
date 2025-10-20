using Japdeva.APIMovil.Colas.Entities;

namespace Japdeva.APIMovil.Colas.Services.EstadoMensajeService
{
    /// <summary>
    /// Interfaz para el servicio de gestión de estados de mensajes en colas.
    /// </summary>
    public interface IEstadoMensajeService
    {
        /// <summary>
        /// Llena el caché de estados de mensajes con datos activos desde el repositorio.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        Task LlenarCacheEstadosMensajeAsync(string traceId);

        /// <summary>
        /// Obtiene un estado de mensaje específico por su identificador desde el caché.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <param name="id">Identificador único del estado de mensaje a buscar.</param>
        /// <returns>La entidad de estado de mensaje encontrada o null si no existe.</returns>
        EstadoMensajeEntity? ObtenerEstadoMensajePorId(string traceId, int id);
    }
}