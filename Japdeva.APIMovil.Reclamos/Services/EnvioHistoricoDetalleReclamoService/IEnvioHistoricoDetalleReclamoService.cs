namespace Japdeva.APIMovil.Reclamos.Services.EnvioHistoricoDetalleReclamoService
{
    /// <summary>
    /// Define la funcionalidad para enviar el histórico de detalles de un reclamo.
    /// </summary>
    public interface IEnvioHistoricoDetalleReclamoService
    {
        /// <summary>
        /// Envía el histórico de detalles de un reclamo específico de forma asíncrona.
        /// </summary>
        /// <param name="traceId">Identificador de traza para seguimiento.</param>
        /// <param name="idReclamo">Identificador del reclamo.</param>
        Task EnviarHistoricoDetalleReclamoAsync(string traceId, long idReclamo);
    }
}
