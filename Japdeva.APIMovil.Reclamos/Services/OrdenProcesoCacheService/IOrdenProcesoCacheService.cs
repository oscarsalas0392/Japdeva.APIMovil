using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.OrdenProcesoCacheService
{
    /// <summary>
    /// Interfaz que define el contrato para el servicio de cache de órdenes de proceso.
    /// Proporciona operaciones para gestionar el cache en memoria de las configuraciones de órdenes de proceso.
    /// </summary>
    public interface IOrdenProcesoCacheService
    {
        /// <summary>
        /// Llena el cache con las configuraciones de orden de proceso activas desde la base de datos.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        Task LlenarCacheOrdenProcesoAsync(string traceId);

        /// <summary>
        /// Obtiene una configuración de orden de proceso específica desde el cache en memoria.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idOrdenProceso">Identificador de la orden de proceso a buscar.</param>
        /// <returns>La entidad de orden de proceso si se encuentra, null en caso contrario.</returns>
        OrdenProcesoEntity? ObtenerOrdenProcesoPorId(string traceId, int idOrdenProceso);

        /// <summary>
        /// Obtiene una orden de proceso específica por su número de orden desde el cache en memoria.
        /// Realiza una búsqueda thread-safe en el cache para encontrar la configuración de una etapa del workflow según el número de orden.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="orden">Número de orden de la etapa del proceso a buscar.</param>
        /// <returns>La entidad de orden de proceso si se encuentra y está activa, null en caso contrario.</returns>
        OrdenProcesoEntity? ObtenerOrdenProcesoPorOrden(string traceId, int orden);
    }
}
