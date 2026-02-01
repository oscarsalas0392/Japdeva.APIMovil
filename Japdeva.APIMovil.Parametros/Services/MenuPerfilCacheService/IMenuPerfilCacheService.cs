using Japdeva.APIMovil.Parametros.Entities;

namespace Japdeva.APIMovil.Parametros.Services.MenuPerfilCacheService
{
    /// <summary>
    /// Define los métodos para la gestión de la caché de menús por perfil, incluyendo la carga y obtención de menús asociados a perfiles específicos.
    /// </summary>
    public interface IMenuPerfilCacheService
    {

        /// <summary>
        /// Llena la caché de menús por perfil obteniendo los datos desde el repositorio y almacenándolos en memoria.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        Task LlenarCacheMenuPerfilAsync(string traceId);

        /// <summary>
        /// Obtiene la lista de menús asociados a un perfil específico desde la caché.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idPefil">Identificador del perfil para el cual se desean obtener los menús.</param>
        /// <returns>Lista de entidades <see cref="MenuPerfilEntity"/> asociadas al perfil especificado.</returns>
        List<MenuPerfilEntity> ObtenerMenuPerfilPorIdPerfil(string traceId, int idPefil);
    }
}
