using Japdeva.APIMovil.Parametros.Entities;

namespace Japdeva.APIMovil.Parametros.Services.TipoMensajeCacheService
{
    /// <summary>
    /// Interfaz para el servicio de caché de tipos de mensaje.
    /// </summary>
    public interface ITipoMensajeCacheService
    {
        /// <summary>
        /// Llena la caché de tipos de mensaje obteniendo la información desde el repositorio remoto.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        Task LlenarCacheTipoMensajesAsync(string traceId);

        /// <summary>
        /// Obtiene un tipo de mensaje por su identificador.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="id">Identificador del tipo de mensaje.</param>
        /// <returns>El tipo de mensaje si existe y está activo, null en caso contrario.</returns>
        TipoMensajeEntity? ObtenerTipoMensajePorId(string traceId, int id);
    }
}
