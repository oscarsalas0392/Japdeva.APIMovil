using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;

namespace Japdeva.APIMovil.Reclamos.Services.ListaRespuestaReclamoService
{
    /// <summary>
    /// Define la interfaz para el servicio que obtiene la lista de respuestas de reclamos.
    /// </summary>
    public interface IListaRespuestaReclamoService
    {
        /// <summary>
        /// Obtiene una lista de modelos de respuesta de reclamo a partir de una lista de entidades de reclamo.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el registro de logs.</param>
        /// <param name="listaReclamoEntity">Lista de entidades de reclamo a procesar.</param>
        /// <returns>Una lista de <see cref="ReclamoRespuestaModel"/> con la información de los reclamos.</returns>
        Task<List<ReclamoRespuestaModel>> ObtenerListaRespuestaReclamoAsync(string traceId, List<ReclamoEntity> listaReclamoEntity);
    }
}
