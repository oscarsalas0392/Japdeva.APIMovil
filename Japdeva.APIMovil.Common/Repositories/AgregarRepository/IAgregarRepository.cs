namespace Japdeva.APIMovil.Common.Repositories.AgregarRepository
{

    /// <summary>
    /// Interfaz para repositorio de agregación de entidades
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a agregar</typeparam>
    public interface IAgregarRepository<T>
    {
        /// <summary>
        /// Agrega una nueva entidad al repositorio
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="entidad">Entidad a agregar</param>
        Task AgregarAsync(string traceId, T entidad);
    }
}