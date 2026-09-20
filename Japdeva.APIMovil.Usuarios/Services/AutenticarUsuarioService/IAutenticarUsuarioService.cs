using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.Services.AutenticarUsuarioService
{
    /// <summary>
    /// Contrato para el servicio de autenticación de usuarios.
    /// </summary>
    public interface IAutenticarUsuarioService
    {
        /// <summary>
        /// Verifica las credenciales del usuario y devuelve sus datos si son válidas.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="solicitud">Credenciales del usuario a autenticar.</param>
        /// <returns>Datos del usuario autenticado o respuesta 401 si las credenciales son inválidas.</returns>
        Task<IActionResult> AutenticarAsync(string traceId, AutenticarUsuarioSolicitudModel solicitud);
    }
}
