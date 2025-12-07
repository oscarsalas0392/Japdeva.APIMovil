using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerReclamoPorDepartamentoService
{
    /// <summary>
    /// Define la interfaz para obtener reclamos por departamento.
    /// </summary>
    public interface IObtenerReclamoPorDepartamentoService
    {
        /// <summary>
        /// Obtiene una lista paginada de reclamos filtrados por departamento.
        /// Consulta los reclamos asociados a un departamento específico y devuelve los resultados con paginación.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idDepartamento">Identificador del departamento por el cual filtrar los reclamos.</param>
        /// <param name="pagina">Número de página para la consulta paginada.</param>
        /// <returns>Un IActionResult con la lista paginada de reclamos del departamento especificado.</returns>
        Task<IActionResult> ObtenerReclamosPorDepartamentoAsync(string traceId, int idDepartamento, int pagina);
    }
}
