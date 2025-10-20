using Japdeva.APIMovil.Colas.Entities;

namespace Japdeva.APIMovil.Colas.Services.MensajeColaService
{
    /// <summary>
    /// Interfaz para el servicio de gestión de mensajes en colas.
    /// </summary>
    public interface IMensajeColaService
    {

        /// <summary>
        /// Llena el caché de mensajes con datos activos desde el repositorio.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <returns>Lista de mensajes de la cola especificada.</returns>
        Task LlenarCacheMensajesAsync(string traceId);
        /// <summary>
        /// Obtiene mensajes por nombre de cola.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <param name="nombreCola">Nombre de la cola a buscar.</param>
        /// <returns>Lista de mensajes encontrados en la cola especificada.</returns>
        Task<List<MensajeColaEntity>> ObtenerMensajesNombreColaAsync(string traceId, string nombreCola);

        /// <summary>
        /// Cuenta la cantidad de mensajes pendientes y fallidos en el sistema.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <returns>Número de mensajes pendientes y fallidos.</returns>
        Task<int> ContarMensajesPendientesAsync(string traceId);

    }
}