namespace Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository
{
    /// <summary>
    /// Interfaz para repositorio de consulta de listas de entidades
    /// </summary>
    /// <typeparam name="T">Tipo de entidad a consultar</typeparam>
    public interface IConsultarListaRepository
    {
        /// <summary>
        /// Consulta una lista de entidades con paginación y filtro opcional.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="pagina">Número de página a consultar (1-based)</param>
        /// <param name="tamanioPagina">Tamaño de la página</param>
        /// <param name="filtro">Expresión de filtro para la consulta (opcional)</param>
        /// <returns>Modelo de respuesta con la lista de entidades y metadatos de paginación</returns>
        Task<Models.RespuestaListaModel<T>> ConsultarListaAsync<T>(string traceId, int pagina, System.Linq.Expressions.Expression<Func<T, bool>>? filtro = null) where T : class;
    }
}