

using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.OrdenNivelProcesoCacheService
{
    /// <summary>
    /// Servicio para gestionar la caché de entidades OrdenNivelProceso.
    /// </summary>
    public interface IOrdenNivelProcesoCacheService 
    {
        /// <summary>
        /// Llena el caché de entidades <see cref="OrdenNivelProcesoEntity"/> activas relacionadas con los niveles de proceso de órdenes.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        Task LlenarCacheOrdenNivelProcesoAsync(string traceId);

        /// <summary>
        /// Obtiene una lista de entidades <see cref="OrdenNivelProcesoEntity"/> del caché que corresponden al identificador del nivel superior especificado.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idNivelSuperior">Identificador del nivel superior para filtrar las entidades.</param>
        /// <returns>Lista de entidades <see cref="OrdenNivelProcesoEntity"/> que coinciden con el nivel superior proporcionado.</returns>
        List<OrdenNivelProcesoEntity> ObtenerOrdenesNivelesProcesoCache(string traceId, int idNivelSuperior);

        /// <summary>
        /// Obtiene una entidad <see cref="OrdenNivelProcesoEntity"/> del caché que corresponde a los identificadores de nivel superior e inferior especificados.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idNivelSuperior">Identificador del nivel superior para filtrar la entidad.</param>
        /// <param name="idNivelInferior">Identificador del nivel inferior para filtrar la entidad.</param>
        /// <returns>Entidad <see cref="OrdenNivelProcesoEntity"/> que coincide con los identificadores proporcionados, o <c>null</c> si no se encuentra.</returns>
        OrdenNivelProcesoEntity? ObtenerOrdenNivelProcesoCache(string traceId, int idNivelSuperior, int idNivelInferior);
    }
}
