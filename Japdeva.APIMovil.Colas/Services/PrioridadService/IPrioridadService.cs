using Japdeva.APIMovil.Colas.Entities;

namespace Japdeva.APIMovil.Colas.Services.PrioridadService
{
    /// <summary>
    /// Interfaz para servicios de gestión de prioridades de mensajes en colas.
    /// </summary>
    public interface IPrioridadService
    {
        /// <summary>
        /// Llena el caché de prioridades con datos activos desde el repositorio.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <returns>Tarea que representa la operación asíncrona.</returns>
        Task LlenarCachePrioridadesAsync(string traceId);

        /// <summary>
        /// Obtiene una prioridad específica por su identificador desde el caché.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <param name="id">Identificador único de la prioridad a buscar.</param>
        /// <returns>La entidad de prioridad encontrada o null si no existe.</returns>
        PrioridadEntity? ObtenerPrioridadPorId(string traceId, int id);

    }
}
