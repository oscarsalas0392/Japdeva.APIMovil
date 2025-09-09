using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Services;
using Japdeva.APIMovil.Common.Models;

namespace Japdeva.APIMovil.Common.Middlewares
{
    /// <summary>
    /// Middleware para validar el token de autenticación en las solicitudes.
    /// </summary>
    public class ValidarTokenMiddleware
    {
        private const string TRACE_ID = "SYSTEM";
        private const string HEADER_AUTHORIZATION = "Authorization";
        private const string MENSAJE_TOKEN_NO_PROPORCIONADO = "Token no proporcionado.";
        private const string MENSAJE_TOKEN_INVALIDO = "Token inválido.";
        private const string MENSAJE_ERROR_PARAMETROS = "Revise la configuración de los parámetros de autenticación en las variables de entorno.(ISSUER)";
        private const string ISSUER_ENV_VARIABLE = "ISSUER";
        private const string AUDIENCE_ENV_VARIABLE = "AUDIENCE";
        private const string CLAVE_SECRETA_ENV_VARIABLE = "CLAVE_SECRETA";
        private readonly string _issuer = Environment.GetEnvironmentVariable(ISSUER_ENV_VARIABLE) ?? string.Empty;
        private readonly string _audience = Environment.GetEnvironmentVariable(AUDIENCE_ENV_VARIABLE) ?? string.Empty;
        private readonly string _claveSecreta = Environment.GetEnvironmentVariable(CLAVE_SECRETA_ENV_VARIABLE) ?? string.Empty;
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidarTokenMiddleware> _logger;
        private readonly IValidarTokenService _validarTokenService;

        /// <summary>
        /// Inicializa una nueva instancia del middleware de validación de token.
        /// </summary>
        /// <param name="next">Siguiente middleware en el pipeline.</param>
        /// <param name="logger">Logger para registrar información.</param>
        /// <param name="validarTokenService">Servicio para validar tokens.</param>
        public ValidarTokenMiddleware(RequestDelegate next, ILogger<ValidarTokenMiddleware> logger, IValidarTokenService validarTokenService)
        {
            this._next = next;
            this._logger = logger;
            this._validarTokenService = validarTokenService;
        }

        /// <summary>
        /// Método de invocación del middleware que valida el token.
        /// </summary>
        /// <param name="context">Contexto HTTP de la solicitud.</param>
        /// <returns>Task que representa la operación asíncrona.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                RespuestaModel respuestaModel = new RespuestaModel();
                respuestaModel.Identificador = context.TraceIdentifier;
                respuestaModel.Exito = false;
                this._logger.Inicio(TRACE_ID, nombreMetodo);

                string? token = context.Request.Headers[HEADER_AUTHORIZATION].FirstOrDefault()?.Split(" ").Last();
                if (string.IsNullOrEmpty(token))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    respuestaModel.Mensaje = MENSAJE_TOKEN_NO_PROPORCIONADO;
                    await context.Response.WriteAsJsonAsync(respuestaModel);
                    return;
                }

                if (string.IsNullOrEmpty(this._issuer) || string.IsNullOrEmpty(this._audience) || string.IsNullOrEmpty(this._claveSecreta))
                {
                    this._logger.Error(TRACE_ID, nombreMetodo, MENSAJE_ERROR_PARAMETROS);
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    respuestaModel.Mensaje = MENSAJE_TOKEN_INVALIDO;
                    await context.Response.WriteAsJsonAsync(respuestaModel);
                    return;
                }

                var claims = this._validarTokenService.ValidarToken(TRACE_ID, token, this._issuer, this._audience, this._claveSecreta);
                if (claims is null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    respuestaModel.Mensaje = MENSAJE_TOKEN_INVALIDO;
                    await context.Response.WriteAsJsonAsync(respuestaModel);
                    return;
                }

                await this._next(context);
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
    }
}
