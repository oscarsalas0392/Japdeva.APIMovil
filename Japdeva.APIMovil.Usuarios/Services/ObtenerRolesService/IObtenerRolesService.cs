using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Usuarios.Services.ObtenerRolesService
{
    /// <summary>
    /// Contrato para el servicio de consulta de roles del sistema.
    /// </summary>
    public interface IObtenerRolesService
    {
        /// <summary>
        /// Retorna todos los roles activos almacenados en caché.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        Task<IActionResult> ObtenerTodosLosRolesAsync(string traceId);

        /// <summary>
        /// Retorna el rol activo con el identificador indicado.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Identificador del rol.</param>
        Task<IActionResult> ObtenerRolPorIdAsync(string traceId, int id);
    }
}
