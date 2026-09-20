using Japdeva.APIMovil.Colas.Entities;

namespace Japdeva.APIMovil.Colas.Repositories.MensajesColaRepository
{
    /// <summary>
    /// Interfaz para el repositorio de operaciones de mensajes en colas.
    /// </summary>
    public interface IMensajesColaRepository
    {
        /// <summary>
        /// Obtiene mensajes pendientes o fallidos excluyendo los ya presentes en caché.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="idsExcluir">IDs de mensajes ya en caché que deben excluirse del resultado.</param>
        /// <returns>Lista de mensajes pendientes o fallidos no presentes en caché.</returns>
        Task<List<MensajeColaEntity>> ObtenerMensajesPendientesAsync(string traceId, IReadOnlyCollection<long> idsExcluir);

        /// <summary>
        /// Devuelve qué IDs de una lista dada siguen siendo Pendiente o Fallido en la base de datos.
        /// Se usa para detectar mensajes del caché que ya fueron procesados.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="ids">IDs a verificar.</param>
        /// <returns>Conjunto de IDs que aún están en estado Pendiente o Fallido.</returns>
        Task<HashSet<long>> ObtenerIdsPendientesEnListaAsync(string traceId, IReadOnlyCollection<long> ids);

        /// <summary>
        /// Marca como expirados en una sola operación SQL todos los mensajes en estado Pendiente,
        /// EnProceso o Fallido cuya fecha de registro sea anterior a la fecha límite indicada.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="fechaLimite">Fecha a partir de la cual un mensaje se considera expirado.</param>
        /// <returns>Cantidad de mensajes actualizados.</returns>
        Task<int> ExpirarMensajesAsync(string traceId, DateTime fechaLimite);

        /// <summary>
        /// Marca como expirados los mensajes RPC (prioridad Alta con IdRpc no vacío) en estado Pendiente,
        /// EnProceso o Fallido cuya fecha de registro sea anterior a la fecha límite indicada.
        /// Estos mensajes tienen un tiempo de vida corto porque el consumidor espera respuesta activamente.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="fechaLimite">Fecha a partir de la cual un mensaje RPC se considera expirado.</param>
        /// <returns>Cantidad de mensajes RPC actualizados.</returns>
        Task<int> ExpirarMensajesRpcAsync(string traceId, DateTime fechaLimite);
    }
}