using Japdeva.APIMovil.Colas.Entities;

namespace Japdeva.APIMovil.Colas.Services.ColaService
{
    /// <summary>
    /// Interfaz para el servicio de gestión de colas en el sistema.
    /// </summary>
    public interface IColaService
    {
        /// <summary>
        /// Llena el caché de colas con datos activos desde el repositorio.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        Task LlenarCacheColasAsync(string traceId);

        /// <summary>
        /// Obtiene una cola específica por su identificador desde el caché.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <param name="nombreCola">Nombre  de la cola a buscar.</param>
        /// <returns>La entidad de cola encontrada o null si no existe.</returns>
        Task<ColaEntity?> ObtenerColaPorNombreAsync(string traceId, string nombreCola);

        /// <summary>
        /// Obtiene una cola específica por su identificador desde el caché en memoria.
        /// </summary>
        /// <param name="id">Identificador de la cola a buscar.</param>
        /// <returns>La entidad de cola encontrada o null si no existe en el caché.</returns>
        ColaEntity? ObtenerColaPorId(long id);

        /// <summary>
        /// Cuenta el número total de colas disponibles en el sistema.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <returns>El número total de colas como una tarea que representa la operación asíncrona.</returns>
        Task<int> ContarColasActivasAsync(string traceId);
    }
}