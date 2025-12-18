using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerDetalleReclamoHistoricoService
{
    /// <summary>
    /// Define el contrato para obtener el detalle histórico de un reclamo.
    /// </summary>
    public interface IObtenerDetalleReclamoHistoricoService
    {
        /// <summary>
        /// Obtiene el detalle histórico de un reclamo específico.
        /// </summary>
        /// <param name="traceId">Identificador de traza para seguimiento.</param>
        /// <param name="idReclamo">Identificador único del reclamo.</param>
        /// <param name="pagina">Número de página para la paginación de resultados.</param>
        /// <returns>Una acción de resultado que contiene el detalle del reclamo.</returns>
        Task<IActionResult> ObtenerDetalleReclamoAsync(string traceId, long idReclamo, int pagina);
    }
}
