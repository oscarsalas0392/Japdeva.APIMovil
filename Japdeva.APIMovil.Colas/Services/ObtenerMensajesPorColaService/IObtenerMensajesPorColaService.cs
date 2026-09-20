using Microsoft.AspNetCore.Mvc;

namespace Japdeva.APIMovil.Colas.Services.ObtenerMensajesPorColaService
{
    /// <summary>
    /// Interfaz para el servicio de obtención de mensajes por cola.
    /// </summary>
    public interface IObtenerMensajesPorColaService
    {
        /// <summary>
        /// Obtiene los mensajes de una cola específica de manera asíncrona.
        /// </summary>
        /// <param name="traceId">El identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <param name="nombreCola">El nombre de la cola de la cual obtener los mensajes.</param>
        /// <returns>Una tarea que representa la operación asíncrona que contiene el resultado de la acción con los mensajes de la cola.</returns>
        Task<IActionResult> ObtenerMensajesPorColaAsync(string traceId, string nombreCola);
    }
}