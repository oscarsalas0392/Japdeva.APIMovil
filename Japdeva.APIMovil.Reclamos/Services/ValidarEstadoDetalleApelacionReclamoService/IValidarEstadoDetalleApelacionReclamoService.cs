using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.ValidarEstadoDetalleApelacionReclamoService
{
    /// <summary>
    /// Define el contrato para el motor de workflow del detalle de apelación de reclamo.
    /// </summary>
    public interface IValidarEstadoDetalleApelacionReclamoService
    {
        /// <summary>
        /// Valida el estado del detalle de una apelación y ejecuta las acciones correspondientes:
        /// rechazar, finalizar, devolver o avanzar al siguiente nivel de proceso.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="estadoDetalle">Entidad que representa el estado del detalle.</param>
        /// <param name="idApelacionReclamo">Identificador de la apelación.</param>
        /// <param name="idNivelActual">Identificador del nivel actual del proceso.</param>
        /// <param name="idNivelSiguienteProceso">Identificador del siguiente nivel del proceso.</param>
        /// <param name="descripcionResolucion">Descripción de la resolución aplicada.</param>
        Task ValidarEstadoDetalleApelacionReclamoAsync(string traceId, EstadoDetalleReclamoEntity estadoDetalle, long idApelacionReclamo, int idNivelActual, int idNivelSiguienteProceso, string descripcionResolucion);
    }
}
