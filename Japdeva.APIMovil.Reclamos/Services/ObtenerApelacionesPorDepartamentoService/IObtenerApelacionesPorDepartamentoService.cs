using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerApelacionesPorDepartamentoService
{
    /// <summary>
    /// Interfaz que define el contrato para el servicio de obtención de apelaciones activas por departamento.
    /// </summary>
    public interface IObtenerApelacionesPorDepartamentoService
    {
        /// <summary>
        /// Obtiene una lista paginada de apelaciones activas asignadas a un departamento específico.
        /// Se consideran activas las apelaciones en estado Pendiente o EnProceso.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idDepartamento">Identificador del departamento a consultar.</param>
        /// <param name="pagina">Número de página para la consulta paginada.</param>
        /// <returns>Un IActionResult con la lista paginada de apelaciones activas del departamento.</returns>
        Task<IActionResult> ObtenerApelacionesPorDepartamentoAsync(string traceId, int idDepartamento, int pagina);
    }
}
