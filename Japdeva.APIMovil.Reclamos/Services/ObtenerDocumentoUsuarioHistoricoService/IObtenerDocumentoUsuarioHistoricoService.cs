using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerDocumentoUsuarioHistoricoService
{
    /// <summary>
    /// Define la interfaz para obtener documentos históricos de usuario asociados a un reclamo.
    /// </summary>
    public interface IObtenerDocumentoUsuarioHistoricoService
    {
        /// <summary>
        /// Obtiene los documentos de usuario asociados a un reclamo específico de forma paginada.
        /// Recupera todos los documentos externos proporcionados por usuarios para un reclamo determinado.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idReclamo">Identificador del reclamo del cual obtener los documentos.</param>
        /// <param name="pagina">Número de página para la consulta paginada.</param>
        /// <returns>Resultado de la operación con la lista paginada de documentos de usuario.</returns>
        Task<IActionResult> ObtenerDocumentoUsuarioAsync(string traceId, long idReclamo, int pagina);
    }
}
