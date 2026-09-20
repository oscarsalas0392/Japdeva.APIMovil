using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Japdeva.APIMovil.Common.Extensions;

namespace Japdeva.APIMovil.EnvioCorreos.Services.SmtpService
{
    /// <summary>
    /// Servicio para el envío de correos electrónicos a través de SMTP usando MailKit.
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
        private readonly string _remitenteNombre;
        private readonly SecureSocketOptions _opcionesSeguridad;
        private readonly int _timeoutConexionSegundos;
        private const string ENV_HOST = "SMTP_HOST";
        private const string ENV_PUERTO = "SMTP_PUERTO";
        private const string ENV_USUARIO = "SMTP_USUARIO";
        private const string ENV_CONTRASENA = "SMTP_CONTRASENA";
        private const string ENV_REMITENTE = "SMTP_REMITENTE";
        private const string ENV_REMITENTE_NOMBRE = "SMTP_REMITENTE_NOMBRE";
        private const string ENV_USAR_SSL = "SMTP_USAR_SSL";
        private const string ENV_TIMEOUT_CONEXION = "SMTP_TIMEOUT_SEGUNDOS";
        private const int PUERTO_DEFECTO = 587;
        private const int TIMEOUT_CONEXION_DEFECTO = 30;
        private const string REMITENTE_NOMBRE_DEFECTO = "Japdeva";
        private const bool DESCONECTAR_LIMPIAMENTE = true;

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
            this._remitenteNombre = Environment.GetEnvironmentVariable(ENV_REMITENTE_NOMBRE) ?? REMITENTE_NOMBRE_DEFECTO;
            bool puertoParsed = int.TryParse(Environment.GetEnvironmentVariable(ENV_PUERTO), out int puertoVal);
            this._puerto = puertoParsed ? puertoVal : PUERTO_DEFECTO;

            bool timeoutParsed = int.TryParse(Environment.GetEnvironmentVariable(ENV_TIMEOUT_CONEXION), out int timeoutVal);
            this._timeoutConexionSegundos = timeoutParsed ? timeoutVal : TIMEOUT_CONEXION_DEFECTO;

            bool usarSslParsed = bool.TryParse(Environment.GetEnvironmentVariable(ENV_USAR_SSL), out bool usarSslVal);
            bool usarSsl = usarSslParsed && usarSslVal;
            this._opcionesSeguridad = usarSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.Auto;
        }

        /// <summary>
        /// Envía un correo electrónico a través del servidor SMTP configurado usando MailKit.
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

                MimeMessage mensaje = new MimeMessage();
                mensaje.From.Add(new MailboxAddress(this._remitenteNombre, this._remitente));
                mensaje.To.Add(MailboxAddress.Parse(destinatario));
                mensaje.Subject = asunto;

                BodyBuilder bodyBuilder = new BodyBuilder();
                if (esCuerpoHtml)
                    bodyBuilder.HtmlBody = cuerpo;
                else
                    bodyBuilder.TextBody = cuerpo;

                mensaje.Body = bodyBuilder.ToMessageBody();

                using CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(this._timeoutConexionSegundos));
                using SmtpClient cliente = new SmtpClient();
                await cliente.ConnectAsync(this._host, this._puerto, SecureSocketOptions.SslOnConnect, cts.Token);
                await cliente.AuthenticateAsync(this._usuario, this._contrasena, cts.Token);
                await cliente.SendAsync(mensaje, cancellationToken: cts.Token);
                await cliente.DisconnectAsync(DESCONECTAR_LIMPIAMENTE, cts.Token);
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
