using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerDocumentoInternoService
{
    /// <summary>
    /// Define la interfaz para el servicio de obtención de documentos internos.
    /// </summary>
    public interface IObtenerDocumentoInternoService
    {
        /// <summary>
        /// Obtiene los documentos internos asociados a un detalle de reclamo.
        /// </summary>
        /// <param name="traceId">Identificador de traza para seguimiento.</param>
        /// <param name="idDetalleReclamo">Identificador del detalle de reclamo.</param>
        /// <returns>Una acción de resultado con los documentos internos.</returns>
        Task<IActionResult> ObtenerDocumentosInternosAsync(string traceId, long idDetalleReclamo);
    }


}
