using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.EnvioCorreos.Models;

namespace Japdeva.APIMovil.EnvioCorreos.Services.AgregarCorreoService
{
    /// <summary>
    /// Contrato para el servicio de registro de correos en la cola de envío.
    /// </summary>
    public interface IAgregarCorreoService
    {
        /// <summary>
        /// Registra un nuevo correo en la cola de envío.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="solicitud">Datos del correo a registrar.</param>
        /// <returns>Resultado de la operación de registro.</returns>
        Task<IActionResult> AgregarCorreoAsync(string traceId, AgregarCorreoSolicitudModel solicitud);
    }
}
