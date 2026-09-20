using Japdeva.APIMovil.Parametros.Entities;

namespace Japdeva.APIMovil.Parametros.Services.OpcionPantallaCacheService
{
    /// <summary>
    /// Define los métodos para el manejo de caché de opciones de pantalla en la aplicación.
    /// </summary>
    public interface IOpcionPantallaCacheService
    {
        /// <summary>
        /// Llena la caché de opciones de pantalla obteniendo los datos desde el repositorio.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el registro de logs.</param>
        Task LlenarCacheOpcionPantallaAsync(string traceId);

        /// <summary>
        /// Obtiene las opciones de pantalla activas que coinciden con los identificadores indicados.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el registro de logs.</param>
        /// <param name="listaIds">Lista de identificadores a buscar.</param>
        /// <returns>Lista de entidades que cumplen con los criterios.</returns>
        List<OpcionPantallaEntity> ObtenerOpcionPantallaPorId(string traceId, List<int> listaIds);
    }
}
