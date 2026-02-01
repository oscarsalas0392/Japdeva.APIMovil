using Japdeva.APIMovil.Parametros.Entities;

namespace Japdeva.APIMovil.Parametros.Services.MensajeCacheService
{
    /// <summary>
    /// Define los métodos para el manejo de la caché de mensajes en la aplicación.
    /// </summary>
    public interface IMensajeCacheService
    {

        /// <summary>
        /// Llena la caché de mensajes obteniéndolos desde el repositorio remoto de mensajes.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        Task LlenarCacheMensajesAsync(string traceId);


        /// <summary>
        /// Obtiene los mensajes almacenados en caché para una pantalla específica y que estén activos.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idPantalla">Identificador de la pantalla para filtrar los mensajes.</param>
        /// <returns>Lista de mensajes activos asociados a la pantalla especificada.</returns>
        List<MensajeEntity> ObtenerMensajes(string traceId, int idPantalla);
    }
}
