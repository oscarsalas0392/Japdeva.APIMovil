namespace Japdeva.APIMovil.Reclamos.Services.UsuarioInternoNombreCacheService
{
    /// <summary>
    /// Servicio de caché de nombres de usuarios internos con refresco periódico.
    /// Almacena en memoria los nombres obtenidos vía RPC y los mantiene actualizados
    /// mediante el servicio de parámetros en segundo plano.
    /// </summary>
    public interface IUsuarioInternoNombreCacheService
    {
        /// <summary>
        /// Carga el caché con los nombres de todos los usuarios internos asignados
        /// a detalles de reclamo, consultando la base de datos y resolviendo nombres vía RPC.
        /// Invocado periódicamente por el servicio de parámetros en segundo plano.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        Task LlenarCacheUsuarioInternoNombreAsync(string traceId);

        /// <summary>
        /// Obtiene el nombre completo de un usuario interno desde el caché en memoria.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idUsuario">Identificador del usuario interno a buscar.</param>
        /// <returns>El nombre completo del usuario, o null si no está en caché.</returns>
        string? ObtenerNombrePorId(string traceId, long idUsuario);
    }
}
