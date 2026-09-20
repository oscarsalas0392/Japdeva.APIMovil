using Japdeva.APIMovil.Parametros.Entities;

namespace Japdeva.APIMovil.Parametros.Services.PlantillaCorreoCacheService
{
    /// <summary>
    /// Interfaz para el servicio de caché de plantillas de correo.
    /// </summary>
    public interface IPlantillaCorreoCacheService
    {
        /// <summary>
        /// Llena la caché de plantillas de correo obteniendo la información desde el repositorio.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        Task LlenarCachePlantillaCorreosAsync(string traceId);

        /// <summary>
        /// Obtiene una plantilla de correo por su identificador.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="id">Identificador de la plantilla de correo.</param>
        /// <returns>La plantilla de correo si existe y está activa, null en caso contrario.</returns>
        PlantillaCorreoEntity? ObtenerPlantillaCorreoPorId(string traceId, int id);
    }
}
