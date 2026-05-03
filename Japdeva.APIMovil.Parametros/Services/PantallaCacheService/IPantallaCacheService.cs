using Japdeva.APIMovil.Parametros.Entities;

namespace Japdeva.APIMovil.Parametros.Services.PantallaCacheService
{
    /// <summary>
    /// Interfaz para el servicio de caché de pantallas.
    /// </summary>
    public interface IPantallaCacheService
    {
        /// <summary>
        /// Llena la caché de pantallas obteniendo la información desde el repositorio remoto.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        Task LlenarCachePantallaAsync(string traceId);

        
        /// <summary>
        /// Obtiene las pantallas por una lista de IDs.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="listaIds">Lista de IDs de pantallas a obtener.</param>
        /// <returns>Lista de pantallas activas que coinciden con los IDs proporcionados.</returns>
        List<PantallaEntity> ObtenerPantallaPorListaId(string traceId, List<int> listaIds);
    }
}
