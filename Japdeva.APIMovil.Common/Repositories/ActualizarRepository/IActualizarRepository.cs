namespace Japdeva.APIMovil.Common.Repositories.ActualizarRepository
{
    /// <summary>
    /// Interfaz para repositorio de actualización de entidades
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a actualizar</typeparam>
    public interface IActualizarRepository
    {
        /// <summary>
        /// Actualiza una entidad existente en el repositorio
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="entidad">Entidad a actualizar</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        Task ActualizarAsync<T>(string traceId, T entidad) where T : class;
    }
}