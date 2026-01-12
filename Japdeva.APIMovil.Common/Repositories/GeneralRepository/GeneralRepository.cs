using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;


namespace Japdeva.APIMovil.Common.Repositories.GeneralRepository
{
    /// <summary>
    /// Repositorio general para operaciones de base de datos comunes y manejo de transacciones.
    /// Proporciona funcionalidades centralizadas para el control de transacciones de Entity Framework
    /// con logging completo y manejo de errores estandarizado.
    /// </summary>
    public class GeneralRepository:IGeneralRepository
    {
        private readonly DbContext _context;
        private readonly ILogger<GeneralRepository> _logger;

        /// <summary>
        /// Inicializa una nueva instancia del repositorio general.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos y errores del repositorio.</param>
        /// <param name="context">Contexto de base de datos de Entity Framework.</param>
        public GeneralRepository(ILogger<GeneralRepository> logger, DbContext context)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Inicia una nueva transacción de base de datos de forma asíncrona.
        /// Proporciona control transaccional para operaciones que requieren atomicidad.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <returns>Una tarea que representa la operación asíncrona. El resultado contiene la transacción iniciada.</returns>
        /// <exception cref="Exception">Se lanza cuando ocurre un error al iniciar la transacción.</exception>
        public async Task<IDbContextTransaction> ObtenerTransaccionBaseDatosAsync(string traceId) 
        {
            string nombreMetodo = this._logger.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                return await this._context.Database.BeginTransactionAsync();

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
        /// Confirma los cambios de una transacción de base de datos de forma asíncrona.
        /// Hace permanentes todas las operaciones realizadas dentro de la transacción.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="transaccion">La transacción de base de datos a confirmar.</param>
        public async Task RealizarCommitBaseDatosAsync(string traceId, IDbContextTransaction transaccion)
        {
            string nombreMetodo = this._logger.ObtenerNombreMetodo();
            try
            {
                 this._logger.Inicio(traceId, nombreMetodo);
                 await transaccion.CommitAsync();
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
        /// Revierte los cambios de una transacción de base de datos de forma asíncrona.
        /// Deshace todas las operaciones realizadas dentro de la transacción.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="transaccion">La transacción de base de datos a revertir. Puede ser null.</param>
        public async Task RealizarDevolucionCambiosBaseDatosAsync(string traceId, IDbContextTransaction? transaccion)
        {
            string nombreMetodo = this._logger.ObtenerNombreMetodo();
            try
            {  
                this._logger.Inicio(traceId, nombreMetodo);
                if (transaccion is not null) await transaccion.RollbackAsync();

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
        /// Libera los recursos de una transacción de base de datos de forma asíncrona.
        /// Realiza la limpieza y disposal de la transacción para evitar memory leaks.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="transaccion">La transacción de base de datos a limpiar. Puede ser null.</param>
        public async Task LimpiarTransaccionAsync(string traceId, IDbContextTransaction? transaccion)
        {
            string nombreMetodo = this._logger.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (transaccion is not null)  await transaccion.DisposeAsync();

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
