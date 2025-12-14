
namespace Japdeva.APIMovil.Reclamos.Services.AgregarReclamoDetalleService
{
    /// <summary>
    /// Interfaz que define el contrato para el servicio de agregación de detalles de reclamos.
    /// Proporciona operaciones para añadir información detallada y seguimiento a reclamos existentes.
    /// </summary>
    public interface IAgregarReclamoDetalleService
    {
        /// <summary>
        /// Agrega un nuevo detalle a un reclamo existente de forma asíncrona.
        /// Permite registrar información adicional, comentarios o actualizaciones sobre el estado del reclamo.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idReclamo">Identificador del reclamo al cual se agregará el detalle.</param>
        /// <param name="idNivelSiguienteProceso">Identificador del nivel de proceso asociado</param>
        Task AgregarReclamoDetalleAsync(string traceId, long idReclamo, int idNivelSiguienteProceso);
    }
}
