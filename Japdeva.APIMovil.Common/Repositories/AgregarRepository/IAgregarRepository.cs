namespace Japdeva.APIMovil.Common.Repositories.AgregarRepository
{

    /// <summary>
    /// Interfaz para repositorio de agregación de entidades
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a agregar</typeparam>
    public interface IAgregarRepository
    {
        /// <summary>
        /// Agrega una nueva entidad al repositorio
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="entidad">Entidad a agregar</param>
        Task AgregarAsync<T>(string traceId, T entidad) where T : class;

        /// <summary>
        /// Agrega múltiples entidades al repositorio en una sola operación
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="entidades">Lista de entidades a agregar</param>
        Task AgregarVariosAsync<T>(string traceId, List<T> entidades) where T : class;
    }
}