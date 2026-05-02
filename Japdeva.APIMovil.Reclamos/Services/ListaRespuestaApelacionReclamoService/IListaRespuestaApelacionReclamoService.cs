using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;

namespace Japdeva.APIMovil.Reclamos.Services.ListaRespuestaApelacionReclamoService
{
    /// <summary>
    /// Interfaz que define el contrato para convertir entidades de apelación en modelos de respuesta.
    /// </summary>
    public interface IListaRespuestaApelacionReclamoService
    {
        /// <summary>
        /// Convierte una lista de entidades de apelación en modelos de respuesta enriquecidos.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="listaApelacionEntity">Lista de entidades de apelación a procesar.</param>
        /// <returns>Lista de <see cref="ApelacionReclamoRespuestaModel"/> con la información de las apelaciones.</returns>
        Task<List<ApelacionReclamoRespuestaModel>> ObtenerListaRespuestaApelacionReclamoAsync(string traceId, List<ApelacionReclamoEntity> listaApelacionEntity);
    }
}
