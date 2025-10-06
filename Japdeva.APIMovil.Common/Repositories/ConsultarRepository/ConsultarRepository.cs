using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;

namespace Japdeva.APIMovil.Common.Repositories.ConsultarRepository
{
    /// <summary>
    /// Repositorio para consultar entidades de tipo T desde la base de datos.
    /// </summary>
    public class ConsultarRepository<T> : IConsultarRepository<T> where T : class
    {
        private readonly DbContext _context;
        private readonly ILogger<ConsultarRepository<T>> _logger;
        private const string MENSAJE_ERROR_TRACE_ID_VACIO = "El traceId no puede estar vacío.";
        private const string MENSAJE_ERROR_FILTRO_NULO = "El filtro no puede ser nulo.";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ConsultarRepository{T}"/>.
        /// </summary>
        /// <param name="context">El contexto de la base de datos.</param>
        public ConsultarRepository(ILogger<ConsultarRepository<T>> logger, DbContext context)
        {
            _context = context;
            _logger = logger; 
        }

        /// <summary>
        /// Consulta una entidad basada en un filtro.
        /// </summary>
        /// <param name="traceId">El identificador de seguimiento.</param>
        /// <param name="filtro">El filtro para encontrar la entidad.</param>
        /// <returns>La entidad encontrada o null si no existe.</returns>
        public async Task<T?> ConsultarAsync(string traceId, System.Linq.Expressions.Expression<Func<T, bool>> filtro)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {                     
                if (string.IsNullOrWhiteSpace(traceId))
                {
                    throw new ArgumentException(MENSAJE_ERROR_TRACE_ID_VACIO, nameof(traceId));
                }

                this._logger.Inicio(traceId, nombreMetodo); 

                if (filtro is null)
                {
                    throw new ArgumentNullException(MENSAJE_ERROR_FILTRO_NULO);
                }                 
                return await this._context.Set<T>().FirstOrDefaultAsync(filtro);
            }
            catch (Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(traceId, nombreMetodo);    
            }
        }
    }
}