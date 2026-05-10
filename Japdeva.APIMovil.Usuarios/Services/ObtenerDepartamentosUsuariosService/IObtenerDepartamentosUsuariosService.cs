using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Usuarios.Services.ObtenerDepartamentosUsuariosService
{
    /// <summary>
    /// Contrato para el servicio de consulta del departamento asignado a un usuario.
    /// </summary>
    public interface IObtenerDepartamentosUsuariosService
    {
        /// <summary>
        /// Retorna el departamento activo asignado al usuario indicado, incluyendo su descripción.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="idUsuario">Identificador del usuario.</param>
        /// <returns>El departamento activo del usuario con su descripción, o NotFound si no tiene asignación.</returns>
        Task<IActionResult> ObtenerDepartamentoPorUsuarioAsync(string traceId, int idUsuario);
    }
}
