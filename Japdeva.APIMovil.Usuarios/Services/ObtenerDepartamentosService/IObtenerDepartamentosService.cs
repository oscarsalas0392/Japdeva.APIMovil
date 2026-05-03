using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Usuarios.Services.ObtenerDepartamentosService
{
    /// <summary>
    /// Contrato para el servicio de consulta de departamentos.
    /// </summary>
    public interface IObtenerDepartamentosService
    {
        /// <summary>
        /// Retorna todos los departamentos activos almacenados en caché.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        Task<IActionResult> ObtenerTodosLosDepartamentosAsync(string traceId);

        /// <summary>
        /// Retorna el departamento activo con el identificador indicado.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Identificador del departamento.</param>
        Task<IActionResult> ObtenerDepartamentoPorIdAsync(string traceId, int id);
    }
}
