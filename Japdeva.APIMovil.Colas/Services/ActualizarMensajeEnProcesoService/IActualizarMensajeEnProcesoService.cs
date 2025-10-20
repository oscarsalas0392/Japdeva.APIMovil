using Japdeva.APIMovil.Colas.Models;

namespace Japdeva.APIMovil.Colas.Services.ActualizarMensajeEnProcesoService
{
    /// <summary>
    /// Interfaz para servicios de actualización de mensajes en proceso en colas.
    /// </summary>
    public interface IActualizarMensajeEnProcesoService
    {
        /// <summary>
        /// Actualiza un mensaje como en proceso en la cola.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="mensaje">Datos del mensaje en proceso a actualizar</param>
        /// <returns>La entidad del mensaje actualizado</returns>
        Task<MensajeColasRespuestaModel> ActualizarMensajeEnProcesoAsync(string traceId, ActualizarMensajeModel mensaje);
    }
}