using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoCacheService
{
    /// <summary>
    /// Interfaz que define el contrato para el servicio de cache de estados de detalle de reclamo.
    /// Proporciona operaciones para gestionar el cache en memoria de los estados de detalle de reclamos.
    /// </summary>
    public interface IEstadoDetalleReclamoCacheService
    {
        /// <summary>
        /// Llena el cache con los estados de detalle de reclamo activos desde la base de datos.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        Task LlenarCacheEstadoDetalleReclamoAsync(string traceId);

        /// <summary>
        /// Obtiene un estado de detalle de reclamo específico desde el cache en memoria.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idEstadoDetalleReclamo">Identificador del estado de detalle de reclamo a buscar.</param>
        /// <returns>La entidad del estado de detalle de reclamo si se encuentra, null en caso contrario.</returns>
        EstadoDetalleReclamoEntity? ObtenerEstadoDetalleReclamo(string traceId, int idEstadoDetalleReclamo);
    }
}
