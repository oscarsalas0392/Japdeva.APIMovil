using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.ManejarTransicionEstadoReclamoService
{
    /// <summary>
    /// Interfaz para el servicio que maneja la transición de estado del reclamo al editar un detalle.
    /// </summary>
    public interface IManejarTransicionEstadoReclamoService
    {
        /// <summary>
        /// Maneja la transición de estado del reclamo:
        /// cambia a EnProceso si se edita desde el primer nivel con ContinuaProceso,
        /// y delega la validación de avance al siguiente nivel cuando corresponde.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idReclamo">Identificador del reclamo.</param>
        /// <param name="nivelProcesoActual">Nivel de proceso actual del detalle editado.</param>
        /// <param name="estadoDetalle">Entidad del estado del detalle de reclamo.</param>
        /// <param name="idNivelSiguienteProceso">ID del siguiente nivel de proceso, o null si no aplica.</param>
        /// <param name="descripcionResolucion">Descripción de la resolución aplicada.</param>
        Task ManejarTransicionEstadoReclamoAsync(string traceId, long idReclamo, int nivelProcesoActual, EstadoDetalleReclamoEntity estadoDetalle, int? idNivelSiguienteProceso, string descripcionResolucion);
    }
}
