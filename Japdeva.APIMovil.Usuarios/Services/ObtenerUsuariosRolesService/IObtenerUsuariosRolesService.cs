using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Usuarios.Services.ObtenerUsuariosRolesService
{
    /// <summary>
    /// Contrato para el servicio de consulta de roles asignados a usuarios.
    /// </summary>
    public interface IObtenerUsuariosRolesService
    {
        /// <summary>
        /// Retorna el rol activo asignado al usuario indicado.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="idUsuario">Identificador del usuario.</param>
        /// <returns>El rol activo del usuario o NotFound si no tiene rol asignado.</returns>
        Task<IActionResult> ObtenerRolPorUsuarioAsync(string traceId, int idUsuario);
    }
}
