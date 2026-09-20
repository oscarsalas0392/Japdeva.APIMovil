using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Usuarios.Models;


namespace Japdeva.APIMovil.Usuarios.Services.ActualizarUsuarioService
{
    /// <summary>
    /// Interfaz para servicios de actualización de usuarios.
    /// </summary>
    public interface IActualizarUsuarioService
    {
        /// <summary>
        /// Actualiza un usuario existente.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="solicitud">Datos del usuario a actualizar</param>
        /// <returns>Resultado de la operación de actualización</returns>
        Task<IActionResult> ActualizarUsuarioAsync(string traceId, ActualizarUsuarioSolicitudModel solicitud);
    }
}
