using Japdeva.APIMovil.Parametros.Entities;

namespace Japdeva.APIMovil.Parametros.Services.ParametroCacheService
{
    /// <summary>
    /// Interfaz para el servicio de caché de parámetros del sistema.
    /// </summary>
    public interface IParametroCacheService
    {
        /// <summary>
        /// Llena la caché de parámetros obteniendo la información desde el repositorio.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        Task LlenarCacheParametrosAsync(string traceId);

        /// <summary>
        /// Obtiene un parámetro activo por su nombre.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="nombre">Nombre del parámetro a obtener.</param>
        /// <returns>El parámetro si existe y está activo, null en caso contrario.</returns>
        ParametroEntity? ObtenerParametroPorNombre(string traceId, string nombre);
    }
}
