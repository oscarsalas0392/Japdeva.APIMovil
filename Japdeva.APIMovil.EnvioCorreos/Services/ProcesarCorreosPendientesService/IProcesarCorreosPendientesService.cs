namespace Japdeva.APIMovil.EnvioCorreos.Services.ProcesarCorreosPendientesService
{
    /// <summary>
    /// Contrato para el servicio de procesamiento de correos pendientes.
    /// </summary>
    public interface IProcesarCorreosPendientesService
    {
        /// <summary>
        /// Consulta los correos pendientes e intenta enviarlos por SMTP.
        /// Actualiza el estado de cada correo según el resultado del envío.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        Task ProcesarPendientesAsync(string traceId);
    }
}
