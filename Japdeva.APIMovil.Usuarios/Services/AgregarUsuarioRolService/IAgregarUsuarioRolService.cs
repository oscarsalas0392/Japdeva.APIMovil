using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.Services.AgregarUsuarioRolService
{
    /// <summary>
    /// Contrato para el servicio de asignación de roles a usuarios.
    /// </summary>
    public interface IAgregarUsuarioRolService
    {
        /// <summary>
        /// Asigna un rol a un usuario.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="solicitud">Datos de la asignación de rol.</param>
        Task<IActionResult> AgregarUsuarioRolAsync(string traceId, AgregarUsuarioRolSolicitudModel solicitud);
    }
}
