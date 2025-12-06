using Microsoft.EntityFrameworkCore.Storage;

namespace Japdeva.APIMovil.Common.Repositories.GeneralRepository
{
    /// <summary>
    /// Interfaz que define el contrato para el repositorio general de operaciones de base de datos.
    /// Proporciona métodos para el manejo centralizado de transacciones de Entity Framework
    /// con soporte para trazabilidad y operaciones asíncronas.
    /// </summary>
    public interface IGeneralRepository
    {
        /// <summary>
        /// Inicia una nueva transacción de base de datos de forma asíncrona.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <returns>Una tarea que representa la operación asíncrona. El resultado contiene la transacción iniciada.</returns>
        Task<IDbContextTransaction> ObtenerTransaccionBaseDatosAsync(string traceId);

        /// <summary>
        /// Confirma los cambios de una transacción de base de datos de forma asíncrona.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="transaccion">La transacción de base de datos a confirmar.</param>
        /// <returns>Una tarea que representa la operación asíncrona de confirmación.</returns>
        Task RealizarCommitBaseDatosAsync(string traceId, IDbContextTransaction transaccion);

        /// <summary>
        /// Revierte los cambios de una transacción de base de datos de forma asíncrona.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="transaccion">La transacción de base de datos a revertir. Puede ser null.</param>
        /// <returns>Una tarea que representa la operación asíncrona de reversión.</returns>
        Task RealizarDevolucionCambiosBaseDatosAsync(string traceId, IDbContextTransaction? transaccion);

        /// <summary>
        /// Libera los recursos de una transacción de base de datos de forma asíncrona.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="transaccion">La transacción de base de datos a limpiar. Puede ser null.</param>
        /// <returns>Una tarea que representa la operación asíncrona de limpieza.</returns>
        Task LimpiarTransaccionAsync(string traceId, IDbContextTransaction? transaccion);
    }
}
