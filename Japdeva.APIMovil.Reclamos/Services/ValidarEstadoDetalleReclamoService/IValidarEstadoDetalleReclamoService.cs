using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.ValidarEstadoDetalleReclamoService
{
    /// <summary>
    /// Define el contrato para la validación del estado del detalle de un reclamo.
    /// </summary>
    public interface IValidarEstadoDetalleReclamoService
    {
        /// <summary>
        /// Valida el estado del detalle de un reclamo y ejecuta las acciones correspondientes según el proceso actual.
        /// Si el estado indica rechazo o finalización, no realiza ninguna acción adicional.
        /// Si el estado indica devolución, valida la configuración de devolución en el orden de nivel de proceso.
        /// Finalmente, agrega un nuevo detalle de reclamo para el siguiente nivel de proceso.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="estadoDetalle">Entidad que representa el estado del detalle del reclamo.</param>
        /// <param name="idReclamo">Identificador del reclamo.</param>
        /// <param name="idNivelActual">Identificador del nivel actual del proceso.</param>
        /// <param name="idNivelSiguienteProceso">Identificador del siguiente nivel del proceso.</param>
        Task ValidarEstadoDetalleReclamoAsync(string traceId, EstadoDetalleReclamoEntity estadoDetalle, long idReclamo, int idNivelActual, int idNivelSiguienteProceso);
    }
}
