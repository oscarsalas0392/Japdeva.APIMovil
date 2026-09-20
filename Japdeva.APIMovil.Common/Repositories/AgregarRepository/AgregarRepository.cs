using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;


namespace Japdeva.APIMovil.Common.Repositories.AgregarRepository
{
    /// <summary>
    /// Repositorio para agregar entidades de tipo T a la base de datos.
    /// </summary>
    public class AgregarRepository : IAgregarRepository
    {
        private readonly DbContext _context;
        private readonly ILogger<AgregarRepository> _logger;
        private const string MENSAJE_ERROR_AGREGAR = "No se pudo agregar la entidad {0} a la base de datos.";
        private const string MENSAJE_ERROR_ENTIDAD_NULA = "La entidad no puede ser nula.";
        private const int MINIMO_REGISTROS_AFECTADOS = 0;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AgregarRepository{T}"/>.
        /// </summary>
        /// <param name="logger">El logger para registro de operaciones.</param>
        /// <param name="context">El contexto de la base de datos.</param>
        public AgregarRepository(ILogger<AgregarRepository> logger, DbContext context)
        {
            this._context = context;
            this._logger = logger;
        }

        /// <summary>
        /// Convierte a UTC todas las propiedades DateTime con Kind=Unspecified directamente en la entidad.
        /// Necesario porque Npgsql rechaza escribir Kind=Unspecified en columnas 'timestamp with time zone'.
        /// </summary>
        /// <typeparam name="T">Tipo de la entidad.</typeparam>
        /// <param name="traceId">Identificador de trazabilidad de la operación.</param>
        /// <param name="entidad">La entidad cuyos DateTime se normalizarán.</param>
        public void NormalizarFechasUtc<T>(string traceId, T entidad) where T : class
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                foreach (PropertyInfo prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!prop.CanRead || !prop.CanWrite) continue;

                    if (prop.PropertyType == typeof(DateTime))
                    {
                        DateTime val = (DateTime)prop.GetValue(entidad)!;
                        if (val.Kind == DateTimeKind.Unspecified)
                            prop.SetValue(entidad, DateTime.SpecifyKind(val, DateTimeKind.Utc));
                    }
                    else if (prop.PropertyType == typeof(DateTime?))
                    {
                        DateTime? val = (DateTime?)prop.GetValue(entidad);
                        if (val.HasValue && val.Value.Kind == DateTimeKind.Unspecified)
                            prop.SetValue(entidad, (DateTime?)DateTime.SpecifyKind(val.Value, DateTimeKind.Utc));
                    }
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

        /// <summary>
        /// Agrega una entidad a la base de datos.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad de la operación.</param>
        /// <param name="entidad">La entidad a agregar.</param>
        public async Task AgregarAsync<T>(string traceId, T entidad) where T : class
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (entidad is null)
                {
                    throw new ArgumentNullException(MENSAJE_ERROR_ENTIDAD_NULA);
                }

                NormalizarFechasUtc(traceId, entidad);
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

        /// <summary>
        /// Agrega múltiples entidades a la base de datos.
        /// </summary>
        /// <typeparam name="T">El tipo de las entidades a agregar.</typeparam>
        /// <param name="traceId">Identificador de trazabilidad de la operación.</param>
        /// <param name="entidades">La lista de entidades a agregar.</param>
        public async Task AgregarVariosAsync<T>(string traceId, List<T> entidades) where T : class
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (entidades is null || !entidades.Any())
                {
                    throw new ArgumentNullException(MENSAJE_ERROR_ENTIDAD_NULA);
                }

                foreach (T entidad in entidades)
                    NormalizarFechasUtc(traceId, entidad);

                this._context.Set<T>().AddRange(entidades);
                int exitosa = await this._context.SaveChangesAsync();
                if (exitosa <= MINIMO_REGISTROS_AFECTADOS)
                {
                    throw new InvalidOperationException(string.Format(MENSAJE_ERROR_AGREGAR, typeof(T).Name));
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
