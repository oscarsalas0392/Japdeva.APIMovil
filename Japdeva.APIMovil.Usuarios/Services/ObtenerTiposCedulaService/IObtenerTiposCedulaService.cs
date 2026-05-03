using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Usuarios.Services.ObtenerTiposCedulaService
{
    /// <summary>
    /// Contrato para el servicio de consulta de tipos de cédula.
    /// </summary>
    public interface IObtenerTiposCedulaService
    {
        /// <summary>
        /// Retorna todos los tipos de cédula activos almacenados en caché.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        Task<IActionResult> ObtenerTodosLosTiposCedulaAsync(string traceId);

        /// <summary>
        /// Retorna el tipo de cédula activo con el identificador indicado.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Identificador del tipo de cédula.</param>
        Task<IActionResult> ObtenerTipoCedulaPorIdAsync(string traceId, int id);
    }
}
