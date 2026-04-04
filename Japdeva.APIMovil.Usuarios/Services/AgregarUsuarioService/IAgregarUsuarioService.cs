using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.Services.AgregarUsuarioService
{
    /// <summary>
    /// Contrato para el servicio de registro de nuevos usuarios.
    /// </summary>
    public interface IAgregarUsuarioService
    {
        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="solicitud">Datos del usuario a registrar.</param>
        Task<IActionResult> AgregarUsuarioAsync(string traceId, AgregarUsuarioSolicitudModel solicitud);

        /// <summary>
        /// Valida que los campos requeridos del modelo de solicitud estén completos y con formato correcto.
        /// </summary>
        /// <param name="solicitud">Datos del usuario a validar.</param>
        void ValidarCamposRequeridos(AgregarUsuarioSolicitudModel solicitud);
    }
}
