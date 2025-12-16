using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoOrdenProcesoCacheService
{
    /// <summary>
    /// Interfaz que define el contrato para el servicio de cache de estados de detalle de reclamo por orden de proceso.
    /// Proporciona operaciones para gestionar el cache en memoria de los estados de detalle de reclamos organizados por orden de proceso.
    /// </summary>
    public interface IEstadoDetalleReclamoOrdenProcesoCacheService
    {
        /// <summary>
        /// Llena el cache con los estados de detalle de reclamo organizados por orden de proceso desde la base de datos.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        Task LlenarCacheEstadoDetalleReclamoOrdenProcesoAsync(string traceId);

        /// <summary>
        /// Obtiene un estado de detalle de reclamo específico por orden de proceso desde el cache en memoria.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idOrdenProceso">Identificador del orden de proceso a buscar.</param>
        /// <param name="idEstadoDetalleReclamo">Identificador del estado de detalle de reclamo para buscar la relación específica.</param>
        /// <returns>La entidad del estado de detalle de reclamo por orden de proceso si se encuentra, null en caso contrario.</returns>
        EstadoDetalleReclamoNivelProcesoEntity? ObtenerEstadoDetalleReclamoOrdenProceso(string traceId, int idOrdenProceso, int idEstadoDetalleReclamo);

        /// <summary>
        /// Obtiene una lista paginada de relaciones entre estados de detalle y órdenes de proceso desde el cache en memoria.
        /// Permite aplicar un filtro opcional para refinar los resultados devueltos.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="pagina">Número de página a recuperar.</param>
        /// <param name="filtro">Función opcional para filtrar los elementos de la lista.</param>
        /// <returns>Un modelo de respuesta que contiene la lista paginada de entidades encontradas.</returns>
        public RespuestaListaModel<EstadoDetalleReclamoNivelProcesoEntity> ObtenerLista(string traceId, int pagina, Func<EstadoDetalleReclamoNivelProcesoEntity, bool>? filtro = null);
    }
}
