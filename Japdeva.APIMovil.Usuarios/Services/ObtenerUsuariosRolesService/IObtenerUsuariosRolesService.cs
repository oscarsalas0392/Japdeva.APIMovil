using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Usuarios.Services.ObtenerUsuariosRolesService
{
    /// <summary>
    /// Contrato para el servicio de consulta de roles asignados a usuarios.
    /// </summary>
    public interface IObtenerUsuariosRolesService
    {
        /// <summary>
        /// Retorna los roles asignados al usuario indicado.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="idUsuario">Identificador del usuario.</param>
        /// <param name="pagina">Número de página.</param>
        Task<IActionResult> ObtenerRolesPorUsuarioAsync(string traceId, int idUsuario, int pagina);
    }
}
