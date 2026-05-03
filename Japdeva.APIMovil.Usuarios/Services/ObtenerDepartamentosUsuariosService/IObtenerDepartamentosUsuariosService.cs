using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Usuarios.Services.ObtenerDepartamentosUsuariosService
{
    /// <summary>
    /// Contrato para el servicio de consulta de departamentos asignados a usuarios.
    /// </summary>
    public interface IObtenerDepartamentosUsuariosService
    {
        /// <summary>
        /// Retorna los departamentos asignados al usuario indicado.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="idUsuario">Identificador del usuario.</param>
        /// <param name="pagina">Número de página.</param>
        Task<IActionResult> ObtenerDepartamentosPorUsuarioAsync(string traceId, int idUsuario, int pagina);
    }
}
