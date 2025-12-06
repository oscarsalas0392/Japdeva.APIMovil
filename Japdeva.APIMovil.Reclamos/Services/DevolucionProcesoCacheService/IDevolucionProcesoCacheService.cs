using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.DevolucionProcesoCacheService
{
    /// <summary>
    /// Interfaz que define el contrato para el servicio de cache de devoluciones de proceso.
    /// Proporciona operaciones para gestionar el cache en memoria de las configuraciones de devolución de procesos.
    /// </summary>
    public interface IDevolucionProcesoCacheService
    {
        /// <summary>
        /// Llena el cache con las configuraciones de devolución de proceso activas desde la base de datos.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        Task LlenarCacheDevolucionProcesoAsync(string traceId);

        /// <summary>
        /// Obtiene una configuración de devolución de proceso específica desde el cache en memoria.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idDevolucionProceso">Identificador de la devolución de proceso a buscar.</param>
        /// <returns>La entidad de devolución de proceso si se encuentra, null en caso contrario.</returns>
        DevolucionProcesoEntity? ObtenerDevolucionProceso(string traceId, int idDevolucionProceso);
    }
}
