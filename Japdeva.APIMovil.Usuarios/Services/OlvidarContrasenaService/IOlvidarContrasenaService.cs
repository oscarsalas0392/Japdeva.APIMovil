using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.Services.OlvidarContrasenaService
{
    /// <summary>
    /// Contrato para el servicio de recuperación de contraseña.
    /// </summary>
    public interface IOlvidarContrasenaService
    {
        /// <summary>
        /// Genera una contraseña temporal y la asigna al usuario identificado por el correo recibido.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="solicitud">Datos de la solicitud con el correo del usuario.</param>
        /// <returns>Resultado de la operación.</returns>
        Task<IActionResult> OlvidarContrasenaAsync(string traceId, OlvidarContrasenaSolicitudModel solicitud);
    }
}
