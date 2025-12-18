using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerDocumentoUsuarioService
{
    /// <summary>
    /// Define la funcionalidad para obtener documentos de usuario asociados a un reclamo específico.
    /// </summary>
    public interface IObtenerDocumentoUsuarioService
    {
        /// <summary>
        /// Obtiene los documentos de usuario asociados a un reclamo específico de forma paginada.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idReclamo">Identificador del reclamo.</param>
        /// <param name="pagina">Número de página a consultar.</param>
        /// <returns>Una respuesta con la lista de documentos de usuario.</returns>
        Task<IActionResult> ObtenerDocumentoUsuarioAsync(string traceId, long idReclamo, int pagina);
    }
}
