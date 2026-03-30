using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Services;

namespace Japdeva.APIMovil.Common.Middlewares
{
    /// <summary>Middleware que envía el token del cliente en el header X-Token de la respuesta.</summary>
    public class GestionarTokenRespuestaMiddleware : IGestionarTokenRespuestaMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GestionarTokenRespuestaMiddleware> _logger;
        private readonly IGenerarTokenService _generarTokenService;
        private readonly IValidarTokenService _validarTokenService;
        private readonly string _rutaAutenticar = Environment.GetEnvironmentVariable(RUTA_AUTENTICAR_ENV) ?? string.Empty;
        private readonly string _issuer = Environment.GetEnvironmentVariable(ISSUER_ENV) ?? string.Empty;
        private readonly string _audience = Environment.GetEnvironmentVariable(AUDIENCE_ENV) ?? string.Empty;
        private readonly string _claveSecreta = Environment.GetEnvironmentVariable(CLAVE_SECRETA_ENV) ?? string.Empty;
        private readonly string _rolCliente = Environment.GetEnvironmentVariable(ROL_CLIENTE_ENV) ?? string.Empty;
        private const string RUTA_AUTENTICAR_ENV = "RUTA_AUTENTICAR";
        private const string ISSUER_ENV = "ISSUER";
        private const string AUDIENCE_ENV = "AUDIENCE";
        private const string CLAVE_SECRETA_ENV = "CLAVE_SECRETA";
        private const string ROL_CLIENTE_ENV = "ROL_CLIENTE";
        private const string ITEM_CLIENTE_TOKEN = "ClienteToken";
        private const string HEADER_TOKEN = "X-Token";
        private const string TRACE_ID = "SYSTEM";
        private const long POSICION_INICIAL = 0;
        private const int HTTP_OK = 200;
        private const char SEPARADOR_RUTA = '/';

        /// <summary>
        /// Inicializa una nueva instancia del middleware de gestión de token en la respuesta.
        /// </summary>
        /// <param name="next">Siguiente middleware en el pipeline.</param>
        /// <param name="logger">Logger para registrar información.</param>
        /// <param name="generarTokenService">Servicio para generar tokens.</param>
        /// <param name="validarTokenService">Servicio para validar tokens.</param>
        public GestionarTokenRespuestaMiddleware(RequestDelegate next, ILogger<GestionarTokenRespuestaMiddleware> logger, IGenerarTokenService generarTokenService, IValidarTokenService validarTokenService)
        {
            this._next = next;
            this._logger = logger;
            this._generarTokenService = generarTokenService;
            this._validarTokenService = validarTokenService;
        }

        /// <summary>
        /// Método de invocación del middleware que gestiona el token en la respuesta.
        /// </summary>
        /// <param name="context">Contexto HTTP de la solicitud.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            string traceId = context.TraceIdentifier;
            try
            {
                this._logger.Inicio(TRACE_ID, nombreMetodo);
                string[] segmentos = context.Request.Path.ToString().Split(SEPARADOR_RUTA);
                bool esRutaAutenticar = !string.IsNullOrEmpty(this._rutaAutenticar)
                    && segmentos.Any(s => s.Equals(this._rutaAutenticar, StringComparison.OrdinalIgnoreCase));
                string tokenEntrada = context.Items[ITEM_CLIENTE_TOKEN]?.ToString() ?? string.Empty;
                Stream streamOriginal = context.Response.Body;
                using MemoryStream streamTemporal = new MemoryStream();
                context.Response.Body = streamTemporal;
                await this._next(context);
                streamTemporal.Seek(POSICION_INICIAL, SeekOrigin.Begin);
                string contenido = await new StreamReader(streamTemporal).ReadToEndAsync();
                context.Response.Body = streamOriginal;
                string nuevoToken = this.ObtenerNuevoToken(traceId, esRutaAutenticar, tokenEntrada, context.Response.StatusCode);
                if (!string.IsNullOrEmpty(nuevoToken))
                    context.Response.Headers[HEADER_TOKEN] = nuevoToken;
                context.Response.ContentLength = null;
                await context.Response.WriteAsync(contenido);
            }
            catch (Exception ex)
            {
                this._logger.Error(TRACE_ID, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(TRACE_ID, nombreMetodo);
            }
        }

        /// <summary>
        /// Genera o refresca el token del cliente según el contexto de la solicitud.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="esRutaAutenticar">Indica si la ruta es la de autenticación.</param>
        /// <param name="tokenEntrada">Token recibido en la solicitud.</param>
        /// <param name="statusCode">Código de estado de la respuesta.</param>
        /// <returns>Nuevo token o cadena vacía si no aplica.</returns>
        public string ObtenerNuevoToken(string traceId, bool esRutaAutenticar, string tokenEntrada, int statusCode)
        {
            string resultado = string.Empty;
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(TRACE_ID, nombreMetodo);
                if (string.IsNullOrEmpty(this._issuer) || string.IsNullOrEmpty(this._audience) || string.IsNullOrEmpty(this._claveSecreta))
                    resultado = string.Empty;
                else if (esRutaAutenticar && statusCode == HTTP_OK)
                    resultado = this._generarTokenService.GenerarToken(traceId, this._issuer, this._audience, this._claveSecreta, this._rolCliente);
                else if (!esRutaAutenticar && !string.IsNullOrEmpty(tokenEntrada))
                    resultado = this.RefrescarToken(traceId, tokenEntrada);
            }
            catch (Exception ex)
            {
                this._logger.Error(TRACE_ID, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(TRACE_ID, nombreMetodo);
            }
            return resultado;
        }

        /// <summary>
        /// Valida el token de entrada y genera un token refrescado con el mismo rol.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="tokenEntrada">Token vigente del cliente.</param>
        /// <returns>Nuevo token refrescado.</returns>
        public string RefrescarToken(string traceId, string tokenEntrada)
        {
            string resultado = string.Empty;
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(TRACE_ID, nombreMetodo);
                var claims = this._validarTokenService.ValidarToken(traceId, tokenEntrada, this._issuer, this._audience, this._claveSecreta);
                string rol = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? this._rolCliente;
                resultado = this._generarTokenService.GenerarToken(traceId, this._issuer, this._audience, this._claveSecreta, rol);
            }
            catch (Exception ex)
            {
                this._logger.Error(TRACE_ID, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(TRACE_ID, nombreMetodo);
            }
            return resultado;
        }
    }
}
