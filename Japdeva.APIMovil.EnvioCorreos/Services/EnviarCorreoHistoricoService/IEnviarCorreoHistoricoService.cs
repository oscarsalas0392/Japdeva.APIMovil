namespace Japdeva.APIMovil.EnvioCorreos.Services.EnviarCorreoHistoricoService
{
    /// <summary>
    /// Contrato para el servicio de movimiento de correos al histórico.
    /// </summary>
    public interface IEnviarCorreoHistoricoService
    {
        /// <summary>
        /// Mueve al histórico los correos enviados exitosamente o con intentos agotados,
        /// y los elimina de la tabla de pendientes.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        Task EnviarHistoricoAsync(string traceId);
    }
}
