namespace Japdeva.APIMovil.Reclamos.Services.EnvioHistoricoDocumentoUsuarioService
{
    /// <summary>
    /// Define la funcionalidad para enviar el histórico de documentos de usuario asociados a un reclamo.
    /// </summary>
    public interface IEnvioHistoricoDocumentoUsuarioService
    {
        /// <summary>
        /// Envía el histórico de documentos del usuario para el reclamo especificado.
        /// </summary>
        /// <param name="traceId">Identificador de traza para seguimiento.</param>
        /// <param name="idReclamo">Identificador del reclamo.</param>
        Task EnviarHistoricoDocumentoUsuarioAsync(string traceId, long idReclamo);
    }
}
