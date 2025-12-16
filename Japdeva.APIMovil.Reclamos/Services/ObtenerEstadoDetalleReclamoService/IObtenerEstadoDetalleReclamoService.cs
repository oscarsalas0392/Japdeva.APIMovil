using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerEstadoDetalleReclamoService
{
    /// <summary>
    /// Define la interfaz para obtener el estado y detalle de un reclamo.
    /// </summary>
    public interface IObtenerEstadoDetalleReclamoService
    {
        /// <summary>
        /// Obtiene el estado y detalle de un reclamo según el identificador de nivel de proceso y la página solicitada.
        /// </summary>
        /// <param name="traceId">Identificador de traza para seguimiento.</param>
        /// <param name="idNivelProceso">Identificador del nivel de proceso.</param>
        /// <param name="pagina">Número de página solicitada.</param>
        /// <returns>Una acción de resultado que contiene el estado y detalle del reclamo.</returns>
        Task<IActionResult> ObtenerEstadoDetalleReclamoAsync(string traceId, int idNivelProceso, int pagina);
    }
}
