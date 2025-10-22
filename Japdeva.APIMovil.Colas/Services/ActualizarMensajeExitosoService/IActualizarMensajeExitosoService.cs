using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Colas.Models;


namespace Japdeva.APIMovil.Colas.Services.ActualizarMensajeExitosoService
{
    /// <summary>
    /// Interfaz para servicios de actualización de mensajes en colas.
    /// </summary>
    public interface IActualizarMensajeExitosoService
    {
        /// <summary>
        /// Actualiza un mensaje existente en la cola.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="mensaje">Datos del mensaje a actualizar</param>
        /// <returns>La entidad del mensaje actualizado</returns>
        Task<IActionResult> ActualizarMensajeExitosoAsync(string traceId, EnviarMensajeSolicitudModel mensaje);

    }
}