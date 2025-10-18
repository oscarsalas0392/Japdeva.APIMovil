using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Services;

namespace Japdeva.APIMovil.Common.Middlewares
{
    /// <summary>
    /// Middleware para validar tokens JWT en las solicitudes HTTP.
    /// </summary>
    public class CrearTokenMiddleware : ICrearTokenMiddleware
    {
    
        private readonly RequestDelegate _next;
        private readonly IGenerarTokenService _generarTokenService;
        private readonly ILogger<CrearTokenMiddleware> _logger;
        private readonly string _rutas = Environment.GetEnvironmentVariable(RUTAS_CON_TOKEN_ENV) ?? string.Empty;
        private const string ISSUER_ENV = "ISSUER_";
        private const string AUDIENCE_ENV = "AUDIENCE_";
        private const string CLAVE_SECRETA_ENV = "CLAVE_SECRETA_";
        private const string ROL_ENV = "ROL_";
        private const string RUTAS_CON_TOKEN_ENV = "RUTAS_CON_TOKEN";
        private const string ERROR_VARIABLES_ENTORNO = "Revise la configuración de los parámetros de autenticación en las variables de entorno.(ISSUER, AUDIENCE, CLAVE_SECRETA, ROL), de la ruta:";
        private const string ERROR_RUTAS_CON_TOKEN_NO_CONFIGURADA = "La variable de entorno RUTAS_CON_TOKEN no está configurada.";
        private const string AUTHORIZATION_HEADER = "Authorization";
        private const string BEARER_PREFIX = "Bearer";
        

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ValidarTokenMiddleware"/>.
        /// </summary>
        /// <param name="next">El siguiente middleware en la tubería de solicitudes.</param>
        /// <param name="validarTokenService">Servicio para validar tokens.</param>
        /// <param name="configuration">Configuración de la aplicación.</param>
        public CrearTokenMiddleware(RequestDelegate next, ILogger<CrearTokenMiddleware> logger, IGenerarTokenService generarTokenService)
        {
            _next = next;
            _logger = logger;
            _generarTokenService = generarTokenService;
        }

        /// <summary>
        /// Procesa una solicitud HTTP y valida el token JWT si está presente.
        /// </summary>
        /// <param name="context">El contexto de la solicitud HTTP.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            List<string> rutasConToken = new List<string>();
            string traceId = context.TraceIdentifier;
            string rol = string.Empty;
            string issuer = string.Empty;
            string audience = string.Empty;
            string claveSecreta = string.Empty;
            string rutaEncontrada = string.Empty;
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                if (string.IsNullOrEmpty(this._rutas))
                {
                    throw new ArgumentException(ERROR_RUTAS_CON_TOKEN_NO_CONFIGURADA);
                }
                rutasConToken = this._rutas.Split(',').Select(r => r.Trim()).ToList();

                foreach (var ruta in rutasConToken)
                {
                    string rutaRequest = context.Request.Path.ToString();
                    if (rutaRequest.Contains(ruta))
                    {
                        rutaEncontrada = ruta;
                        issuer = Environment.GetEnvironmentVariable(ISSUER_ENV + ruta.ToUpper()) ?? string.Empty;
                        audience = Environment.GetEnvironmentVariable(AUDIENCE_ENV + ruta.ToUpper()) ?? string.Empty;
                        claveSecreta = Environment.GetEnvironmentVariable(CLAVE_SECRETA_ENV + ruta.ToUpper()) ?? string.Empty;
                        rol = Environment.GetEnvironmentVariable(ROL_ENV + ruta.ToUpper()) ?? string.Empty;
                        break;
                    }
                }
                
                if (!string.IsNullOrEmpty(rutaEncontrada))
                {
                    if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience) || string.IsNullOrEmpty(claveSecreta) || string.IsNullOrEmpty(rol))
                    {
                        throw new ArgumentException(ERROR_VARIABLES_ENTORNO + rutaEncontrada);
                    }
                    string token = this._generarTokenService.GenerarToken(traceId, issuer, audience, claveSecreta, rol);
                    token = $"{BEARER_PREFIX} {token}";
                    context.Request.Headers.Append(AUTHORIZATION_HEADER, token);              
                }
                await this._next(context);
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