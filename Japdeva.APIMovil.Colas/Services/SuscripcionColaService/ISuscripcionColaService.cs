using System.Threading.Channels;
using Japdeva.APIMovil.Colas.Models;

namespace Japdeva.APIMovil.Colas.Services.SuscripcionColaService
{
    /// <summary>
    /// Interfaz para el servicio de suscripción a colas mediante canales en memoria.
    /// </summary>
    public interface ISuscripcionColaService
    {
        /// <summary>
        /// Suscribe un nuevo consumidor a una cola y retorna el canal de lectura.
        /// </summary>
        /// <param name="nombreCola">Nombre de la cola a suscribir.</param>
        /// <returns>Canal de lectura para recibir mensajes en tiempo real.</returns>
        ChannelReader<MensajeColasRespuestaModel> Suscribir(string nombreCola);

        /// <summary>
        /// Elimina la suscripción de un consumidor a una cola.
        /// </summary>
        /// <param name="nombreCola">Nombre de la cola.</param>
        /// <param name="reader">Canal de lectura a desuscribir.</param>
        void Desuscribir(string nombreCola, ChannelReader<MensajeColasRespuestaModel> reader);

        /// <summary>
        /// Notifica a todos los suscriptores de una cola con nuevos mensajes disponibles.
        /// </summary>
        /// <param name="nombreCola">Nombre de la cola que recibió nuevos mensajes.</param>
        /// <param name="mensajes">Lista de nuevos mensajes a notificar.</param>
        void NotificarMensajes(string nombreCola, IEnumerable<MensajeColasRespuestaModel> mensajes);

        /// <summary>
        /// Registra una espera puntual por un mensaje con un IdRpc específico.
        /// Cuando el BackgroundService notifique ese mensaje, la tarea se completa sin polling a BD.
        /// </summary>
        /// <param name="idRpc">Identificador RPC del mensaje esperado.</param>
        /// <param name="cancellationToken">Token de cancelación para respetar el timeout del cliente.</param>
        /// <returns>El mensaje cuando llegue, o null si se cancela la espera.</returns>
        Task<MensajeColasRespuestaModel?> EsperarMensajePorIdRpcAsync(string idRpc, CancellationToken cancellationToken);
    }
}
