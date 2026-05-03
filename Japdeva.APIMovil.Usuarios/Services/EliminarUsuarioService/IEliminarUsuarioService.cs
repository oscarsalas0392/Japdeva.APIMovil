using Microsoft.AspNetCore.Mvc;


namespace Japdeva.APIMovil.Usuarios.Services.EliminarUsuarioService
{
    /// <summary>
    /// Interfaz para servicios de eliminación de usuarios.
    /// </summary>
    public interface IEliminarUsuarioService
    {
        /// <summary>
        /// Elimina un usuario por su identificador.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="id">Identificador del usuario a eliminar</param>
        /// <returns>Resultado de la operación de eliminación</returns>
        Task<IActionResult> EliminarUsuarioAsync(string traceId, int id);
    }
}
