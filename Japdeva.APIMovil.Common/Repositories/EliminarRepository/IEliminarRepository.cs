namespace Japdeva.APIMovil.Common.Repositories.EliminarRepository
{
    /// <summary>
    /// Interfaz para repositorio de eliminación de entidades
    /// </summary>
    public interface IEliminarRepository
    {
        /// <summary>
        /// Elimina una lista de entidades de la base de datos
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="entidades">Lista de entidades a eliminar</param>
        /// <returns>Tarea que representa la operación asíncrona</returns>
        Task EliminarVariosAsync<T>(string traceId, List<T> entidades) where T : class;

        /// <summary>
        /// Elimina una entidad de la base de datos.
        /// </summary>
        /// <param name="traceId">El identificador de seguimiento.</param>
        /// <param name="entidad">La entidad a eliminar.</param>
        Task EliminarAsync<T>(string traceId, T entidad) where T : class;

    }
}