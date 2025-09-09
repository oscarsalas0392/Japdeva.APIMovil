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
        private const string USUARIO_PATH = "Usuarios";
        private const string ISSUER_USUARIO_ENV = "ISSUER_USUARIO";
        private const string AUDIENCE_USUARIO_ENV = "AUDIENCE_USUARIO";
        private const string CLAVE_SECRETA_USUARIO_ENV = "CLAVE_SECRETA_USUARIO";
        private const string ROL_ENV = "ROL_USUARIO";
        private readonly RequestDelegate _next;
        private readonly IGenerarTokenService _generarTokenService;
        private readonly ILogger<CrearTokenMiddleware> _logger;
        private const string ERROR_VARIABLES_ENTORNO_USUARIOS = "Revise la configuración de los parámetros de autenticación en las variables de entorno.(ISSUER_USUARIO, AUDIENCE_USUARIO, CLAVE_SECRETA_USUARIO)";
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
        /// <returns>Una tarea que representa el procesamiento de la solicitud.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            string traceId = context.TraceIdentifier;
            string variableEntornoIssuer = string.Empty;
            string variableEntornoAudience = string.Empty;
            string variableEntornoClaveSecreta= string.Empty;
            string variableEntornoRol = string.Empty;
            string errorVariablesEntorno = string.Empty;
            string rol = string.Empty;
            string issuer ;
            string audience;
            string claveSecreta;
            try
            {
                if (context.Request.Path.Equals(USUARIO_PATH, StringComparison.OrdinalIgnoreCase))
                {
                    variableEntornoIssuer = ISSUER_USUARIO_ENV;
                    variableEntornoAudience = AUDIENCE_USUARIO_ENV;
                    variableEntornoClaveSecreta = CLAVE_SECRETA_USUARIO_ENV;
                    variableEntornoRol = ROL_ENV;
                    errorVariablesEntorno = ERROR_VARIABLES_ENTORNO_USUARIOS;
                }
                issuer = Environment.GetEnvironmentVariable(variableEntornoIssuer) ?? string.Empty;
                audience = Environment.GetEnvironmentVariable(variableEntornoAudience) ?? string.Empty;
                claveSecreta = Environment.GetEnvironmentVariable(variableEntornoClaveSecreta) ?? string.Empty;
                if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience) || string.IsNullOrEmpty(claveSecreta))
                {
                        throw new ArgumentException(errorVariablesEntorno);
                }
                string token = this._generarTokenService.GenerarToken(traceId, rol, issuer, audience, claveSecreta);
                token = $"{BEARER_PREFIX} {token}";
                context.Request.Headers.Append(AUTHORIZATION_HEADER, token);
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