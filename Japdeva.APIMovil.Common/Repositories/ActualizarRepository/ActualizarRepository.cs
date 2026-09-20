using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;

namespace Japdeva.APIMovil.Common.Repositories.ActualizarRepository
{
    /// <summary>
    /// Repositorio para actualizar entidades de tipo T en la base de datos.
    /// </summary>
    public class ActualizarRepository : IActualizarRepository
    {
        private readonly DbContext _context;
        private readonly ILogger<ActualizarRepository> _logger;
        private const string MENSAJE_ERROR_ACTUALIZAR = "No se pudo actualizar la entidad {0} en la base de datos.";
        private const string MENSAJE_ERROR_ENTIDAD_NULA = "La entidad no puede ser nula.";
        private const string MENSAJE_ERROR_TRACE_ID_VACIO = "El traceId no puede estar vacío.";
        private const int MINIMO_REGISTROS_AFECTADOS = 0;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ActualizarRepository{T}"/>.
        /// </summary>
        /// <param name="logger">El logger para registrar operaciones.</param>
        /// <param name="context">El contexto de la base de datos.</param>
        /// <exception cref="ArgumentNullException">Cuando logger o context son null.</exception>
        public ActualizarRepository(ILogger<ActualizarRepository> logger, DbContext context)
        {
            this._logger = logger;
            this._context = context;
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
        /// Actualiza una entidad en la base de datos de forma asíncrona.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad de la operación.</param>
        /// <param name="entidad">La entidad a actualizar.</param>
        /// <exception cref="ArgumentNullException">Cuando la entidad es nula.</exception>
        /// <exception cref="ArgumentException">Cuando el traceId está vacío.</exception>
        /// <exception cref="InvalidOperationException">Cuando no se puede actualizar la entidad.</exception>
        public async Task ActualizarAsync<T>(string traceId, T entidad) where T : class
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (entidad is null)
                {
                    throw new ArgumentNullException(nameof(entidad), MENSAJE_ERROR_ENTIDAD_NULA);
                }

                if (string.IsNullOrWhiteSpace(traceId))
                {
                    throw new ArgumentException(MENSAJE_ERROR_TRACE_ID_VACIO, nameof(traceId));
                }

                NormalizarFechasUtc(traceId, entidad);
                this._context.Set<T>().Update(entidad);
                int exitosa = await this._context.SaveChangesAsync();

                if (exitosa <= MINIMO_REGISTROS_AFECTADOS)
                {
                    throw new InvalidOperationException(string.Format(MENSAJE_ERROR_ACTUALIZAR, entidad.GetType().Name));
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
