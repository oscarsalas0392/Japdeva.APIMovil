using Japdeva.APIMovil.Usuarios.Entities;

namespace Japdeva.APIMovil.Usuarios.Services.TipoCedulaCacheService
{
    /// <summary>
    /// Contrato para la caché de tipos de cédula disponibles.
    /// </summary>
    public interface ITipoCedulaCacheService
    {
        /// <summary>
        /// Carga o recarga todos los tipos de cédula activos desde la base de datos.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        Task LlenarCacheTiposCedulaAsync(string traceId);

        /// <summary>
        /// Retorna todos los tipos de cédula activos almacenados en caché.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        List<TipoCedulaEntity> ObtenerTodos(string traceId);

        /// <summary>
        /// Retorna el tipo de cédula activo con el identificador indicado, o null si no existe.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Identificador del tipo de cédula.</param>
        TipoCedulaEntity? ObtenerPorId(string traceId, int id);
    }
}
