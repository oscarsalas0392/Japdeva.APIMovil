namespace Japdeva.APIMovil.Common.Repositories.ConsultarRepository
{
    /// <summary>
    /// Interfaz para repositorio de consulta de entidades
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a consultar</typeparam>
    public interface IConsultarRepository
    {
        /// <summary>
        /// Obtiene una entidad por su identificador
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="filtro">Expresión de filtro para la consulta</param>
        /// <returns>La entidad encontrada o null si no existe</returns>
        Task<T?> ConsultarAsync<T>(string traceId, System.Linq.Expressions.Expression<Func<T, bool>> filtro) where T : class;

    }
}
