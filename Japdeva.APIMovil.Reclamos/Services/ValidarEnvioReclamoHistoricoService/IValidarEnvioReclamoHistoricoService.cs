namespace Japdeva.APIMovil.Reclamos.Services.ValidarEnvioReclamoHistoricoService
{
    /// <summary>
    /// Define el contrato para la validación del envío de reclamos históricos.
    /// </summary>
    public interface IValidarEnvioReclamoHistoricoService
    {
        /// <summary>
        /// Valida y realiza el envío histórico de los reclamos según los criterios definidos.
        /// </summary>
        /// <param name="traceId">Identificador de la traza para el seguimiento de la operación.</param>
        Task ValidarEnvioReclamoHistoricoAsync(string traceId);
    }
}
