using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Colas.Models;


namespace Japdeva.APIMovil.Colas.Services.ActualizarMensajeFallidoService
{
    /// <summary>
    /// Interfaz para servicios de actualización de mensajes fallidos en colas.
    /// </summary>
    public interface IActualizarMensajeFallidoService
    {
        /// <summary>
        /// Actualiza un mensaje como fallido en la cola.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="mensaje">Datos del mensaje fallido a actualizar</param>
        /// <returns>La entidad del mensaje actualizado</returns>
        Task<IActionResult> ActualizarMensajeFallidoAsync(string traceId, EnviarMensajeSolicitudModel mensaje);
    }
}