using System.Net;
using System.Net.Mail;
using Japdeva.APIMovil.Common.Extensions;

#pragma warning disable SYSLIB0006, CS0618
namespace Japdeva.APIMovil.EnvioCorreos.Services.SmtpService
{
    /// <summary>
    /// Servicio para el envío de correos electrónicos a través de SMTP.
    /// La configuración se obtiene de variables de entorno al iniciar la aplicación.
    /// </summary>
    public class SmtpService : ISmtpService
    {
        private readonly ILogger<SmtpService> _logger;
        private readonly string _host;
        private readonly int _puerto;
        private readonly string _usuario;
        private readonly string _contrasena;
        private readonly string _remitente;
        private readonly bool _usarSsl;
        private const string ENV_HOST = "SMTP_HOST";
        private const string ENV_PUERTO = "SMTP_PUERTO";
        private const string ENV_USUARIO = "SMTP_USUARIO";
        private const string ENV_CONTRASENA = "SMTP_CONTRASENA";
        private const string ENV_REMITENTE = "SMTP_REMITENTE";
        private const string ENV_USAR_SSL = "SMTP_USAR_SSL";
        private const int PUERTO_DEFECTO = 587;
        private const bool SSL_DEFECTO = true;

        /// <summary>
        /// Inicializa el servicio SMTP leyendo la configuración desde variables de entorno.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        public SmtpService(ILogger<SmtpService> logger)
        {
            this._logger = logger;
            this._host = Environment.GetEnvironmentVariable(ENV_HOST) ?? string.Empty;
            this._usuario = Environment.GetEnvironmentVariable(ENV_USUARIO) ?? string.Empty;
            this._contrasena = Environment.GetEnvironmentVariable(ENV_CONTRASENA) ?? string.Empty;
            this._remitente = Environment.GetEnvironmentVariable(ENV_REMITENTE) ?? string.Empty;
            this._usarSsl = bool.TryParse(Environment.GetEnvironmentVariable(ENV_USAR_SSL), out bool ssl) ? ssl : SSL_DEFECTO;
            this._puerto = int.TryParse(Environment.GetEnvironmentVariable(ENV_PUERTO), out int puerto) ? puerto : PUERTO_DEFECTO;
        }

        /// <summary>
        /// Envía un correo electrónico a través del servidor SMTP configurado.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="destinatario">Dirección de correo del destinatario.</param>
        /// <param name="asunto">Asunto del correo.</param>
        /// <param name="cuerpo">Cuerpo del correo.</param>
        /// <param name="esCuerpoHtml">Indica si el cuerpo está en formato HTML.</param>
        public async Task EnviarAsync(string traceId, string destinatario, string asunto, string cuerpo, bool esCuerpoHtml)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var cliente = new SmtpClient(this._host, this._puerto);
                cliente.EnableSsl = this._usarSsl;
                cliente.Credentials = new NetworkCredential(this._usuario, this._contrasena);
                using var mensaje = new MailMessage(this._remitente, destinatario, asunto, cuerpo);
                mensaje.IsBodyHtml = esCuerpoHtml;
                await cliente.SendMailAsync(mensaje);
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
#pragma warning restore SYSLIB0006, CS0618
