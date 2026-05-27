using Japdeva.APIMovil.Parametros.Entities;

namespace Japdeva.APIMovil.Parametros.Services.OpcionPantallaPerfilCacheService
{
    /// <summary>
    /// Define los métodos para el manejo de caché de opciones de pantalla por perfil en la aplicación.
    /// </summary>
    public interface IOpcionPantallaPerfilCacheService
    {
        /// <summary>
        /// Llena la caché de opciones de pantalla por perfil obteniendo los datos desde el repositorio.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el registro de logs.</param>
        Task LlenarCacheOpcionPantallaPerfilAsync(string traceId);

        /// <summary>
        /// Obtiene las opciones de pantalla asociadas a un perfil específico desde la caché.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el registro de logs.</param>
        /// <param name="idPerfil">Identificador del perfil.</param>
        /// <returns>Lista de entidades asociadas al perfil especificado.</returns>
        List<OpcionPantallaPerfilEntity> ObtenerOpcionPantallaPerfilPorIdPerfil(string traceId, int idPerfil);
    }
}
