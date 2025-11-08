using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Colas.Models;

namespace Japdeva.APIMovil.Colas.Services.ObtenerMensajePorIdRpcService
{
    /// <summary>
    /// Interfaz para servicios de obtención de mensajes de cola por identificador RPC.
    /// </summary>
    public interface IObtenerMensajePorIdRpcService
    {
        /// <summary>
        /// Busca un mensaje en la cola por su identificador RPC.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad de la operación.</param>
        /// <param name="nombreCola">Nombre de la cola donde buscar el mensaje.</param>
        /// <param name="idRpc">Identificador RPC del mensaje a buscar.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con el resultado del mensaje encontrado.</returns>
        Task<MensajeColasRespuestaModel> BuscarIdRpcAsync(string traceId, string nombreCola, string idRpc);

        /// <summary>
        /// Obtiene un mensaje de la cola por su identificador RPC.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad de la operación.</param>
        /// <param name="idRpc">Identificador RPC del mensaje a obtener.</param>
        /// <param name="nombreCola">Nombre de la cola donde obtener el mensaje.</param>
        /// <returns>Una tarea que representa la operación asíncrona, con el resultado de la acción.</returns>
        Task<IActionResult> ObtenerMensajeRpcAsync(string traceId, string idRpc, string nombreCola);
    }
    
}