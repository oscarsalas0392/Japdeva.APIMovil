using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;


namespace Japdeva.APIMovil.Common.Repositories.AgregarRepository
{
    /// <summary>
    /// Repositorio para agregar entidades de tipo T a la base de datos.
    /// </summary>
    public class AgregarRepository<T> : IAgregarRepository<T> where T : class
    {
        private readonly DbContext _context;
        private readonly ILogger<AgregarRepository<T>> _logger;
        private const string MENSAJE_ERROR_AGREGAR = "No se pudo agregar la entidad {0} a la base de datos.";
        private const string MENSAJE_ERROR_ENTIDAD_NULA = "La entidad no puede ser nula.";
        private const int MINIMO_REGISTROS_AFECTADOS = 0;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AgregarRepository{T}"/>.
        /// </summary>
        /// <param name="context">El contexto de la base de datos.</param>
        public AgregarRepository(ILogger<AgregarRepository<T>> logger, DbContext context)
        {
            _context = context;
            _logger = logger; 
        }

        /// <summary>
        /// Agrega una entidad a la base de datos.
        /// </summary>
        /// <param name="entidad">La entidad a agregar.</param>
        /// <param name="traceId">Identificador de trazabilidad de la operación.</param>
        public async Task AgregarAsync(string traceId, T entidad)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                if (entidad is null)
                {
                    throw new ArgumentNullException(MENSAJE_ERROR_ENTIDAD_NULA);
                }
                
                this._logger.Inicio(traceId, nombreMetodo);
                this._context.Set<T>().Add(entidad); 
                int exitosa = await this._context.SaveChangesAsync();
                if (exitosa <= MINIMO_REGISTROS_AFECTADOS)
                {
                    throw new InvalidOperationException(string.Format(MENSAJE_ERROR_AGREGAR, entidad.GetType().Name));
                }
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