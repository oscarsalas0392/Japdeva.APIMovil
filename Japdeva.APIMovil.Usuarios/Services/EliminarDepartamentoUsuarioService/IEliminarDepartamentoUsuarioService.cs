using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Usuarios.Services.EliminarDepartamentoUsuarioService
{
    /// <summary>
    /// Contrato para el servicio de eliminación de asignaciones de usuarios a departamentos.
    /// </summary>
    public interface IEliminarDepartamentoUsuarioService
    {
        /// <summary>
        /// Elimina la asignación de un usuario a un departamento por su identificador.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Identificador de la asignación a eliminar.</param>
        Task<IActionResult> EliminarDepartamentoUsuarioAsync(string traceId, long id);
    }
}
