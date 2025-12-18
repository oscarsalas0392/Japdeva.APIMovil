using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Reclamos.Models;


namespace Japdeva.APIMovil.Reclamos.Services.AgregarDocumentoInternoService
{
    /// <summary>
    /// Define la interfaz para el servicio encargado de agregar documentos internos a un reclamo.
    /// </summary>
    public interface IAgregarDocumentoInternoService
    {
        /// <summary>
        /// Agrega un documento interno asociado a un reclamo.
        /// </summary>
        /// <param name="traceId">Identificador de traza para seguimiento de la operación.</param>
        /// <param name="agregarDocumentoInternoSolicitudModel">Modelo con los datos del documento a agregar.</param>
        Task<IActionResult> AgregarDocumentoInternoAsync(string traceId, AgregarDocumentoInternoSolicitudModel agregarDocumentoInternoSolicitudModel);
    }
}
