
using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Colas.Models;

namespace Japdeva.APIMovil.Colas.Services.EnviarMensajeService
{
    /// <summary>
    /// Interfaz para servicios de envío de mensajes a colas.
    /// </summary>
    public interface IEnviarMensajeService
    {
        /// <summary>
        /// Envía un mensaje a la cola especificada.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="mensaje">Datos del mensaje a enviar</param>
        /// <returns>Resultado de la operación de envío</returns>
        Task<MensajeColaEntity> EnviarMensajeAsync(string traceId, EnviarMensajeModel mensaje);

    }
}