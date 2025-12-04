using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Colas.Services.ActualizarMensajeExpiradoService
{
    /// <summary>
    /// Interfaz para servicios de actualización de mensajes expirados en colas.
    /// </summary>
    public interface IActualizarMensajeExpiradoService
    {
        /// <summary>
        /// Actualiza un mensaje como expirado en la cola.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="idMensaje">Identificador del mensaje a actualizar</param>
        /// <param name="traceIdMensaje">Identificador de trazabilidad del mensaje</param>
        /// <returns>La entidad del mensaje actualizado</returns>
        Task<IActionResult> ActualizarMensajeExpiradoAsync(string traceId, long idMensaje, string traceIdMensaje);
    }
}