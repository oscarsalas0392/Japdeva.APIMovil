using Japdeva.APIMovil.Reclamos.Models;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerEstadoDetalleReclamoListaService
{
    /// <summary>
    /// Interfaz para el servicio que resuelve el estado detalle actual de una lista de reclamos.
    /// </summary>
    public interface IObtenerEstadoDetalleReclamoListaService
    {
        /// <summary>
        /// Obtiene y asigna el estado detalle actual a cada reclamo de la lista,
        /// consultando el detalle más reciente de la BD y resolviendo la descripción desde caché.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="listaRespuesta">Lista de modelos de reclamo a enriquecer.</param>
        /// <returns>La lista con el estado detalle resuelto para cada reclamo.</returns>
        Task<List<ReclamoRespuestaModel>> ObtenerEstadoDetalleReclamoAsync(string traceId, List<ReclamoRespuestaModel> listaRespuesta);
    }
}
