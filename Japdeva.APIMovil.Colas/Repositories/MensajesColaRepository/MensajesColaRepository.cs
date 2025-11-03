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
        /// Obtiene los mensajes pendientes o fallidos de la cola de manera asíncrona.
        /// </summary>
        /// <param name="traceId">El identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <returns>Una lista de entidades de mensajes de cola pendientes o fallidos.</returns>
        public async Task<List<MensajeColaEntity>> ObtenerMensajesPendientesAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                return await this._context.Set<MensajeColaEntity>()
                    .Where(mensaje => mensaje.EstadoId == (int)EstadoMensajeModel.Pendiente ||
                                      mensaje.EstadoId == (int)EstadoMensajeModel.Fallido)
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
    }
}