namespace Japdeva.APIMovil.Reclamos.Services.AgregarApelacionReclamoDetalleService
{
    /// <summary>
    /// Interfaz que define el contrato para el servicio de agregación de detalles de apelación de reclamo.
    /// </summary>
    public interface IAgregarApelacionReclamoDetalleService
    {
        /// <summary>
        /// Agrega un nuevo detalle a una apelación existente de forma asíncrona.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idApelacionReclamo">Identificador de la apelación a la cual se agregará el detalle.</param>
        /// <param name="idNivelProceso">Identificador del nivel de proceso asociado.</param>
        Task AgregarApelacionReclamoDetalleAsync(string traceId, long idApelacionReclamo, int idNivelProceso);
    }
}
