namespace Japdeva.APIMovil.Reclamos.Services.EnvioHistoricoDocumentoInternoService
{
    /// <summary>
    /// Define la funcionalidad para enviar el histórico de documentos internos relacionados con un reclamo.
    /// </summary>
    public interface IEnvioHistoricoDocumentoInternoService
    {
        /// <summary>
        /// Envía el histórico de documentos internos asociados a un detalle de reclamo.
        /// </summary>
        /// <param name="traceId">Identificador de traza para seguimiento de la operación.</param>
        /// <param name="idDetalleReclamo">Identificador del detalle del reclamo.</param>
        Task EnviarHistoricoDocumentoInternoAsync(string traceId, long idDetalleReclamo);
    }
}
