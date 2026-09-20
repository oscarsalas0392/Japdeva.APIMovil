using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerDocumentoInternoPorIdReclamoService
{
    /// <summary>
    /// Interfaz para el servicio de obtención de documentos internos filtrados por identificador de reclamo.
    /// </summary>
    public interface IObtenerDocumentoInternoPorIdReclamoService
    {
        /// <summary>
        /// Obtiene los documentos internos de todos los detalles de un reclamo,
        /// incluyendo la descripción del detalle y el nombre del departamento.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idReclamo">Identificador del reclamo a consultar.</param>
        /// <param name="pagina">Número de página para la paginación de resultados.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        Task<IActionResult> ObtenerDocumentoInternoPorIdReclamoAsync(string traceId, long idReclamo, int pagina);
    }
}
