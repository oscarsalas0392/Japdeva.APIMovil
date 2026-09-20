using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerDetalleReclamoPorIdDetalleService
{
    /// <summary>
    /// Interfaz para el servicio de obtención de un detalle de reclamo por su identificador único.
    /// </summary>
    public interface IObtenerDetalleReclamoPorIdDetalleService
    {
        /// <summary>
        /// Obtiene un detalle de reclamo específico por su identificador único.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idDetalleReclamo">Identificador único del detalle de reclamo a consultar.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        Task<IActionResult> ObtenerDetalleReclamoPorIdDetalleAsync(string traceId, long idDetalleReclamo);
    }
}
