using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.EstadoReclamoCacheService
{
    /// <summary>
    /// Interfaz que define el contrato para el servicio de cache de estados de reclamo.
    /// Proporciona operaciones para gestionar el cache en memoria de los estados de reclamos del sistema.
    /// </summary>
    public interface IEstadoReclamoCacheService
    {
        /// <summary>
        /// Llena el cache con los estados de reclamo activos desde la base de datos.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        Task LlenarCacheEstadoReclamoAsync(string traceId);

        /// <summary>
        /// Obtiene un estado de reclamo específico desde el cache en memoria.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idEstadoReclamo">Identificador del estado de reclamo a buscar.</param>
        /// <returns>La entidad del estado de reclamo si se encuentra, null en caso contrario.</returns>
        EstadoReclamoEntity? ObtenerEstadoReclamo(string traceId, int idEstadoReclamo);
    }
}
