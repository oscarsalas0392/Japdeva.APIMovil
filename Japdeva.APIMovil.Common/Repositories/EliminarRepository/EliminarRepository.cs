using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;

namespace Japdeva.APIMovil.Common.Repositories.EliminarRepository
{
    /// <summary>
    /// Repositorio para eliminar entidades de tipo T desde la base de datos.
    /// </summary>
    public class EliminarRepository : IEliminarRepository 
    {
        private readonly DbContext _context;
        private readonly ILogger<EliminarRepository> _logger;
        private const string MENSAJE_ERROR_TRACE_ID_VACIO = "El traceId no puede estar vacío.";
        private const string MENSAJE_ERROR_ENTIDADES_NULAS = "La lista de entidades no puede ser nula o vacía.";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="EliminarRepository{T}"/>.
        /// </summary>
        /// <param name="context">El contexto de la base de datos.</param>
        public EliminarRepository(ILogger<EliminarRepository> logger, DbContext context)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Elimina una lista de entidades de la base de datos.
        /// </summary>
        /// <param name="traceId">El identificador de seguimiento.</param>
        /// <param name="entidades">Lista de entidades a eliminar.</param>
        public async Task EliminarAsync<T>(string traceId, List<T> entidades) where T : class
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                if (string.IsNullOrWhiteSpace(traceId))
                {
                    throw new ArgumentException(MENSAJE_ERROR_TRACE_ID_VACIO, nameof(traceId));
                }

                this._logger.Inicio(traceId, nombreMetodo);

                if (entidades is null || !entidades.Any())
                {
                    throw new ArgumentNullException(nameof(entidades), MENSAJE_ERROR_ENTIDADES_NULAS);
                }

                this._context.Set<T>().RemoveRange(entidades);
                await this._context.SaveChangesAsync();
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