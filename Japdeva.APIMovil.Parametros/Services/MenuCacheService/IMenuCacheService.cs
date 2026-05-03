using Japdeva.APIMovil.Parametros.Entities;

namespace Japdeva.APIMovil.Parametros.Services.MenuCacheService
{
    /// <summary>
    /// Define los métodos para el manejo de caché de menús en la aplicación.
    /// </summary>
    public interface IMenuCacheService
    {
        /// <summary>
        /// Llena la caché de menús obteniendo los datos desde el repositorio y almacenándolos en memoria.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el registro de logs.</param>
        Task LlenarCacheMenuAsync(string traceId);

        /// <summary>
        /// Obtiene una lista de menús desde la caché que coinciden con los identificadores proporcionados y que están activos.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el registro de logs.</param>
        /// <param name="listaIds">Lista de identificadores de menú a buscar.</param>
        /// <returns>Lista de objetos <see cref="MenuEntity"/> que cumplen con los criterios.</returns>
        List<MenuEntity> ObtenerMenuPorId(string traceId, List<int> listaIds);
    }
}
