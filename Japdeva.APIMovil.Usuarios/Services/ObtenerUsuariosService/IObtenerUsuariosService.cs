using Microsoft.AspNetCore.Mvc;


namespace Japdeva.APIMovil.Usuarios.Services.ObtenerUsuariosService
{
    /// <summary>
    /// Interfaz para servicios de obtenci�n de usuarios.
    /// </summary>
    public interface IObtenerUsuariosService
    {
        /// <summary>
        /// Obtiene todos los usuarios activos.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <returns>Resultado con la lista de usuarios</returns>
        Task<IActionResult> ObtenerTodosLosUsuariosAsync(string traceId);

        /// <summary>
        /// Obtiene un usuario por su identificador.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="id">Identificador del usuario a obtener</param>
        /// <returns>Resultado con el usuario encontrado</returns>
        Task<IActionResult> ObtenerUsuarioPorIdAsync(string traceId, int id);

        /// <summary>
        /// Obtiene un usuario activo por su número de identificación (cédula).
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="identificacion">Número de identificación del usuario a buscar</param>
        /// <returns>Resultado con el usuario encontrado</returns>
        Task<IActionResult> ObtenerUsuarioPorIdentificacionAsync(string traceId, string identificacion);
    }
}
