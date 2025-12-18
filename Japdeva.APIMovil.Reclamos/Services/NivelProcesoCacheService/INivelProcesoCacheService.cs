using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.NivelProcesoCacheService
{
    /// <summary>
    /// Interfaz que define el contrato para el servicio de cache de órdenes de proceso.
    /// Proporciona operaciones para gestionar el cache en memoria de las configuraciones de órdenes de proceso.
    /// </summary>
    public interface INivelProcesoCacheService
    {
        /// <summary>
        /// Llena el cache con las configuraciones de orden de proceso activas desde la base de datos.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        Task LlenarCacheNivelProcesoAsync(string traceId);

        /// <summary>
        /// Obtiene un nivel de proceso específico del cache por su identificador.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idNivelProceso">Identificador del nivel de proceso a buscar.</param>
        /// <returns>
        /// La entidad <see cref="NivelProcesoEntity"/> correspondiente al identificador proporcionado,
        /// o <c>null</c> si no se encuentra un nivel de proceso activo con ese identificador.
        /// </returns>
        NivelProcesoEntity? ObtenerNivelProcesoPorId(string traceId, int idNivelProceso);

        /// <summary>
        /// Obtiene el primer nivel de proceso activo del cache.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <returns>
        /// La entidad <see cref="NivelProcesoEntity"/> correspondiente al primer nivel activo,
        /// o <c>null</c> si no se encuentra un nivel de proceso activo con el nivel inicial.
        /// </returns>
        NivelProcesoEntity? ObtenerPrimerNivel(string traceId);


        /// <summary>
        /// Obtiene una lista paginada de niveles de proceso desde el cache, aplicando un filtro opcional.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="pagina">Número de página a recuperar.</param>
        /// <param name="filtro">Función opcional para filtrar los elementos de la lista.</param>
        /// <returns>
        /// Un modelo <see cref="RespuestaListaModel{NivelProcesoEntity}"/> que contiene la lista paginada de niveles de proceso,
        /// junto con la cantidad total de registros y páginas.
        /// </returns>
        RespuestaListaModel<NivelProcesoEntity> ObtenerLista(string traceId, int pagina, Func<NivelProcesoEntity, bool>? filtro = null);
    }
}
