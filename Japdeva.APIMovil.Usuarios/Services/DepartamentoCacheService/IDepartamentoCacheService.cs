using Japdeva.APIMovil.Usuarios.Entities;

namespace Japdeva.APIMovil.Usuarios.Services.DepartamentoCacheService
{
    /// <summary>
    /// Contrato para la caché de departamentos de la organización.
    /// </summary>
    public interface IDepartamentoCacheService
    {
        /// <summary>
        /// Carga o recarga todos los departamentos activos desde la base de datos.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        Task LlenarCacheDepartamentosAsync(string traceId);

        /// <summary>
        /// Retorna todos los departamentos activos almacenados en caché.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        List<DepartamentoEntity> ObtenerTodos(string traceId);

        /// <summary>
        /// Retorna el departamento activo con el identificador indicado, o null si no existe.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Identificador del departamento.</param>
        DepartamentoEntity? ObtenerPorId(string traceId, int id);
    }
}
