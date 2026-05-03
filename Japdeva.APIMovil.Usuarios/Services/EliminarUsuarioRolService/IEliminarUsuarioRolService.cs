using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Usuarios.Services.EliminarUsuarioRolService
{
    /// <summary>
    /// Contrato para el servicio de eliminación de asignaciones de roles a usuarios.
    /// </summary>
    public interface IEliminarUsuarioRolService
    {
        /// <summary>
        /// Elimina la asignación de un rol a un usuario por su identificador.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Identificador de la asignación a eliminar.</param>
        Task<IActionResult> EliminarUsuarioRolAsync(string traceId, long id);
    }
}
