using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerDetalleReclamoService
{
    /// <summary>
    /// Define la interfaz para obtener el detalle de un reclamo.
    /// </summary>
    public interface IObtenerDetalleReclamoService
    {
        /// <summary>
        /// Obtiene el detalle de un reclamo específico, incluyendo información de usuarios internos y departamentos asociados,
        /// así como la descripción del estado del detalle del reclamo. El resultado es paginado.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idReclamo">Identificador único del reclamo a consultar.</param>
        /// <param name="pagina">Número de página para la paginación de resultados.</param>
        /// <returns>Un <see cref="IActionResult"/> que contiene la respuesta con el detalle del reclamo.</returns>
        Task<IActionResult> ObtenerDetalleReclamoAsync(string traceId, long idReclamo, int pagina);
    }
}
