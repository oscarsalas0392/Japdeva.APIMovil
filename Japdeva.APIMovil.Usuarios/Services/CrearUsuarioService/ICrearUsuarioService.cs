using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.Services.CrearUsuarioService
{
    /// <summary>
    /// Interfaz del servicio para crear usuarios.
    /// </summary>
    public interface ICrearUsuarioService
    {
        /// <summary>
        /// Crea un nuevo usuario.
        /// </summary>
        /// <param name="traceId">El identificador de traza de la solicitud.</param>
        /// <param name="solicitud">Modelo de solicitud con los datos del usuario a crear.</param>
        /// <returns>Respuesta de la creación del usuario.</returns>
        Task<IActionResult> CrearUsuarioAsync(string traceId, CrearUsuarioSolicitudModel solicitud);
    }
}
