using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Reclamos.Services.EliminarDocumentoInternoService
{
    /// <summary>
    /// Define el contrato para los servicios de eliminación de documentos internos.
    /// </summary>
    public interface IEliminarDocumentoInternoService
    {
        /// <summary>
        /// Elimina un documento interno de acuerdo al identificador proporcionado.
        /// </summary>
        /// <param name="traceId">Identificador de traza para seguimiento.</param>
        /// <param name="idDocumento">Identificador único del documento a eliminar.</param>
        /// <returns>Una acción de resultado que indica el resultado de la operación.</returns>
        Task<IActionResult> EliminarDocumentoInternoAsync(string traceId, long idDocumento);
    }
}
