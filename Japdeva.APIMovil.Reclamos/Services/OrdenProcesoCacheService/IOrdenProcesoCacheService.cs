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
        OrdenProcesoEntity? ObtenerOrdenProceso(string traceId, int idOrdenProceso);
    }
}
