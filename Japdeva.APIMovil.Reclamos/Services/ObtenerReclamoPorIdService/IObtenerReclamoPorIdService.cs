using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerReclamoPorIdService
{
    /// <summary>
    /// Define el contrato para el servicio que obtiene un reclamo por su identificador único.
    /// </summary>
    public interface IObtenerReclamoPorIdService
    {
        /// <summary>
        /// Obtiene un reclamo específico por su identificador único.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para la solicitud.</param>
        /// <param name="idReclamo">Identificador único del reclamo a consultar.</param>
        /// <returns>Una acción de resultado que contiene el reclamo encontrado.</returns>
        Task<IActionResult> ObtenerReclamoPorIdAsync(string traceId, long idReclamo);
    }
}
