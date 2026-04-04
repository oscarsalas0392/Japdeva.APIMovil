using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.Services.ActualizarUsuarioRolService
{
    /// <summary>
    /// Contrato para el servicio de actualización de asignaciones de roles a usuarios.
    /// </summary>
    public interface IActualizarUsuarioRolService
    {
        /// <summary>
        /// Actualiza la asignación de un rol a un usuario.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="solicitud">Datos de la actualización.</param>
        /// <returns>Resultado de la operación de actualización.</returns>
        Task<IActionResult> ActualizarUsuarioRolAsync(string traceId, ActualizarUsuarioRolSolicitudModel solicitud);
    }
}
