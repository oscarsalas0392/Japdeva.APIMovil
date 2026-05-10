using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerReclamosPorUsuarioOrdenadoService
{
    /// <summary>
    /// Interfaz que define el contrato para el servicio de obtención de reclamos por usuario ordenados por fecha descendente.
    /// </summary>
    public interface IObtenerReclamosPorUsuarioOrdenadoService
    {
        /// <summary>
        /// Obtiene una lista paginada de todos los reclamos de un usuario, ordenados del más reciente al más antiguo.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idUsuario">Identificador del usuario externo propietario de los reclamos.</param>
        /// <param name="pagina">Número de página para la consulta paginada.</param>
        /// <returns>Un IActionResult con la lista paginada de reclamos ordenados por fecha descendente.</returns>
        Task<IActionResult> ObtenerReclamosPorUsuarioOrdenadoAsync(string traceId, int idUsuario, int pagina);
    }
}
