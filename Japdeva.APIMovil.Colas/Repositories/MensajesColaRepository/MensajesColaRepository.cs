using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Colas.Models;
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
        private const int TAMANIO_LOTE_EXPIRACION = 500;
        private const int TOTAL_INICIAL = 0;
        private const string MENSAJE_EXPIRADO_PREFIJO = "Mensaje expirado - Tiempo límite excedido en ";
        private const string MENSAJE_EXPIRADO_SUFIJO = " UTC";
        private const string FORMATO_FECHA_EXPIRACION = "yyyy-MM-dd HH:mm:ss";

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
        /// Marca como expirados en una sola operación SQL todos los mensajes en estado Pendiente,
        /// EnProceso o Fallido cuya fecha de registro sea anterior a la fecha límite indicada.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="fechaLimite">Fecha a partir de la cual un mensaje se considera expirado.</param>
        /// <returns>Cantidad de mensajes actualizados.</returns>
        public async Task<int> ExpirarMensajesAsync(string traceId, DateTime fechaLimite)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                string mensajeError = $"{MENSAJE_EXPIRADO_PREFIJO}{DateTime.UtcNow.ToString(FORMATO_FECHA_EXPIRACION)}{MENSAJE_EXPIRADO_SUFIJO}";
                int totalActualizado = TOTAL_INICIAL;
                int actualizadosEnLote;
                do
                {
                    actualizadosEnLote = await this._context.Set<MensajeColaEntity>()
                        .Where(m => (m.EstadoId == (int)EstadoMensajeModel.Pendiente ||
                                     m.EstadoId == (int)EstadoMensajeModel.EnProceso ||
                                     m.EstadoId == (int)EstadoMensajeModel.Fallido) &&
                                    m.FechaRegistro <= fechaLimite)
                        .Take(TAMANIO_LOTE_EXPIRACION)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(m => m.EstadoId, (int)EstadoMensajeModel.Expirado)
                            .SetProperty(m => m.FechaEdicion, DateTime.UtcNow)
                            .SetProperty(m => m.MensajeError, mensajeError));
                    totalActualizado += actualizadosEnLote;
                }
                while (actualizadosEnLote == TAMANIO_LOTE_EXPIRACION);
                return totalActualizado;
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
        /// Marca como expirados los mensajes RPC (prioridad Alta con IdRpc no vacío) en estado Pendiente,
        /// EnProceso o Fallido cuya fecha de registro sea anterior a la fecha límite indicada.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="fechaLimite">Fecha a partir de la cual un mensaje RPC se considera expirado.</param>
        /// <returns>Cantidad de mensajes RPC actualizados.</returns>
        public async Task<int> ExpirarMensajesRpcAsync(string traceId, DateTime fechaLimite)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                string mensajeError = $"{MENSAJE_EXPIRADO_PREFIJO}{DateTime.UtcNow.ToString(FORMATO_FECHA_EXPIRACION)}{MENSAJE_EXPIRADO_SUFIJO}";
                int totalActualizado = TOTAL_INICIAL;
                int actualizadosEnLote;
                do
                {
                    actualizadosEnLote = await this._context.Set<MensajeColaEntity>()
                        .Where(m => m.PrioridadId == (int)PrioridadModel.Alta &&
                                    m.IdRpc != string.Empty &&
                                    (m.EstadoId == (int)EstadoMensajeModel.Pendiente ||
                                     m.EstadoId == (int)EstadoMensajeModel.EnProceso ||
                                     m.EstadoId == (int)EstadoMensajeModel.Fallido) &&
                                    m.FechaRegistro <= fechaLimite)
                        .Take(TAMANIO_LOTE_EXPIRACION)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(m => m.EstadoId, (int)EstadoMensajeModel.Expirado)
                            .SetProperty(m => m.FechaEdicion, DateTime.UtcNow)
                            .SetProperty(m => m.MensajeError, mensajeError));
                    totalActualizado += actualizadosEnLote;
                }
                while (actualizadosEnLote == TAMANIO_LOTE_EXPIRACION);
                return totalActualizado;
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