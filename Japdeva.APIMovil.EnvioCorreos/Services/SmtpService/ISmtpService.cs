namespace Japdeva.APIMovil.EnvioCorreos.Services.SmtpService
{
    /// <summary>
    /// Contrato para el servicio de envío de correos por SMTP.
    /// </summary>
    public interface ISmtpService
    {
        /// <summary>
        /// Envía un correo electrónico a través del servidor SMTP configurado.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="destinatario">Dirección de correo del destinatario.</param>
        /// <param name="asunto">Asunto del correo.</param>
        /// <param name="cuerpo">Cuerpo del correo.</param>
        /// <param name="esCuerpoHtml">Indica si el cuerpo está en formato HTML.</param>
        Task EnviarAsync(string traceId, string destinatario, string asunto, string cuerpo, bool esCuerpoHtml);
    }
}
