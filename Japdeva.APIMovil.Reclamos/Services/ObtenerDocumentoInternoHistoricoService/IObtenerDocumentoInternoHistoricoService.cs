using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerDocumentoInternoHistoricoService
{
    /// <summary>
    /// Define la interfaz para el servicio encargado de obtener documentos internos históricos asociados a un detalle de reclamo.
    /// </summary>
    public interface IObtenerDocumentoInternoHistoricoService
    {
        /// <summary>
        /// Obtiene de forma asíncrona los documentos internos históricos para un detalle de reclamo específico y una página determinada.
        /// </summary>
        /// <param name="traceId">Identificador de traza para seguimiento de la solicitud.</param>
        /// <param name="idDetalleReclamo">Identificador del detalle del reclamo.</param>
        /// <param name="pagina">Número de página de los resultados a obtener.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        Task<IActionResult> ObtenerDocumentosInternosAsync(string traceId, long idDetalleReclamo, int pagina);
    }
}
