using Japdeva.APIMovil.Usuarios.Entities;

namespace Japdeva.APIMovil.Usuarios.Services.RolCacheService
{
    /// <summary>
    /// Contrato para la caché de roles del sistema.
    /// </summary>
    public interface IRolCacheService
    {
        /// <summary>
        /// Carga o recarga todos los roles activos desde la base de datos.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        Task LlenarCacheRolesAsync(string traceId);

        /// <summary>
        /// Retorna todos los roles activos almacenados en caché.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        List<RolEntity> ObtenerTodos(string traceId);

        /// <summary>
        /// Retorna el rol activo con el identificador indicado, o null si no existe.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Identificador del rol.</param>
        RolEntity? ObtenerPorId(string traceId, int id);
    }
}
