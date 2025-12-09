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
        /// Agrega detalles por devolución u orden, o rechaza el proceso según los valores de la entidad.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el registro de logs.</param>
        /// <param name="estadoDetalleReclamoEntity">Entidad que representa el estado del detalle del reclamo.</param>
        /// <param name="idReclamo">Identificador único del reclamo.</param>
        /// <param name="idOrdenProcesoActual">Identificador del proceso de orden actual.</param>
        /// <param name="idDevolucionProceso">Identificador del proceso de devolución, si aplica.</param>
        Task ValidarEstadoDetalleReclamoAsync(string traceId, EstadoDetalleReclamoEntity estadoDetalleReclamoEntity, long idReclamo, int idOrdenProcesoActual, int? idDevolucionProceso);
    }
}
