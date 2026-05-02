using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerApelacionesPorFechaEstadoService
{
    /// <summary>
    /// Interfaz que define el contrato para el servicio de obtención de apelaciones por rango de fechas y estado.
    /// </summary>
    public interface IObtenerApelacionesPorFechaEstadoService
    {
        /// <summary>
        /// Obtiene una lista paginada de apelaciones filtradas por rango de fechas de ingreso y estado.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="fechaInicio">Fecha de inicio del rango a consultar.</param>
        /// <param name="fechaFin">Fecha de fin del rango a consultar.</param>
        /// <param name="idEstadoReclamo">Identificador del estado a filtrar.</param>
        /// <param name="pagina">Número de página para la consulta paginada.</param>
        /// <returns>Un IActionResult con la lista paginada de apelaciones que cumplen los criterios.</returns>
        Task<IActionResult> ObtenerApelacionesPorFechaEstadoAsync(string traceId, DateTime fechaInicio, DateTime fechaFin, int idEstadoReclamo, int pagina);
    }
}
