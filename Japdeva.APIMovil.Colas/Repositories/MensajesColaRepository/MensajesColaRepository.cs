using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Colas.Models;
using Japdeva.APIMovil.Colas.Repositories;
using Japdeva.APIMovil.Common.Extensions;

namespace Japdeva.APIMovil.Colas.Repositories.MensajesColaRepository
{
    /// <summary>
    /// Repository class for managing message queue operations.
    /// </summary>
    public class MensajesColaRepository : IMensajesColaRepository
    {
        private readonly DbContext _context;
        private readonly ILogger<MensajesColaRepository> _logger;
        private const int TAMANIO_PAGINA = 50;

        /// <summary>
        /// Initializes a new instance of the <see cref="MensajesColaRepository"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logger instance.</param>
        public MensajesColaRepository(DbContext context, ILogger<MensajesColaRepository> logger)
        {
            _context = context;
            _logger = logger;   
        }
        
        /// <summary>
        /// Obtiene mensajes pendientes o fallidos excluyendo los ya presentes en caché.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="idsExcluir">IDs de mensajes ya en caché que deben excluirse del resultado.</param>
        /// <returns>Lista de mensajes pendientes o fallidos no presentes en caché.</returns>
        public async Task<List<MensajeColaEntity>> ObtenerMensajesPendientesAsync(string traceId, IReadOnlyCollection<long> idsExcluir)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                IQueryable<MensajeColaEntity> query = this._context.Set<MensajeColaEntity>()
                    .Where(mensaje => mensaje.EstadoId == (int)EstadoMensajeModel.Pendiente ||
                                      mensaje.EstadoId == (int)EstadoMensajeModel.Fallido);

                if (idsExcluir.Any())
                    query = query.Where(mensaje => !idsExcluir.Contains(mensaje.Id));

                return await query
                    .OrderBy(mensaje => mensaje.PrioridadId)
                    .ThenBy(mensaje => mensaje.FechaRegistro)
                    .AsNoTracking()
                    .Take(TAMANIO_PAGINA)
                    .ToListAsync();
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
        /// Devuelve qué IDs de una lista dada siguen siendo Pendiente o Fallido en la base de datos.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="ids">IDs a verificar.</param>
        /// <returns>Conjunto de IDs que aún están en estado Pendiente o Fallido.</returns>
        public async Task<HashSet<long>> ObtenerIdsPendientesEnListaAsync(string traceId, IReadOnlyCollection<long> ids)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                List<long> idsPendientes = await this._context.Set<MensajeColaEntity>()
                    .Where(mensaje => ids.Contains(mensaje.Id) &&
                                     (mensaje.EstadoId == (int)EstadoMensajeModel.Pendiente ||
                                      mensaje.EstadoId == (int)EstadoMensajeModel.Fallido))
                    .Select(mensaje => mensaje.Id)
                    .ToListAsync();

                return idsPendientes.ToHashSet();
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