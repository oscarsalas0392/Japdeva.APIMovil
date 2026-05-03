using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.Services.ActualizarContrasenaUsuarioService
{
    /// <summary>
    /// Contrato para el servicio de actualización de contraseña de usuario.
    /// </summary>
    public interface IActualizarContrasenaUsuarioService
    {
        /// <summary>
        /// Actualiza la contraseña de un usuario validando la contraseña anterior.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="solicitud">Datos de la solicitud con la contraseña anterior y la nueva.</param>
        /// <returns>Resultado de la operación.</returns>
        Task<IActionResult> ActualizarContrasenaAsync(string traceId, ActualizarContrasenaUsuarioSolicitudModel solicitud);
    }
}
