using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;

namespace Japdeva.APIMovil.Common.Middlewares.ManejoErroresGatewayMiddleware
{
    /// <summary>
    /// Middleware para el manejo centralizado de errores en el Gateway.
    /// Captura excepciones de token expirado y errores generales.
    /// </summary>
    public class ManejoErroresGatewayMiddleware : IManejoErroresGatewayMiddleware
    {
        private readonly RequestDelegate _siguiente;
        private readonly ILogger<ManejoErroresGatewayMiddleware> _logger;
        private const string TRACE_ID = "SYSTEM";
        private const string MENSAJE_TOKEN_EXPIRADO = "Token expirado.";
        private const string MENSAJE_ERROR_INTERNO = "La solicitud ha fallado.";
        private const bool ERROR = false;

        /// <summary>
        /// Inicializa una nueva instancia del middleware de manejo de errores del Gateway.
        /// </summary>
        /// <param name="siguiente">Siguiente middleware en el pipeline.</param>
        /// <param name="logger">Logger para registrar errores.</param>
        public ManejoErroresGatewayMiddleware(RequestDelegate siguiente, ILogger<ManejoErroresGatewayMiddleware> logger)
        {
            this._siguiente = siguiente;
            this._logger = logger;
        }

        /// <summary>
        /// Método de invocación del middleware que maneja las excepciones del Gateway.
        /// </summary>
        /// <param name="contextoHttp">Contexto HTTP de la solicitud.</param>
        public async Task InvokeAsync(HttpContext contextoHttp)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(TRACE_ID, nombreMetodo);
                await this._siguiente(contextoHttp);
            }
            catch (SecurityTokenExpiredException ex)
            {
                this._logger.Error(TRACE_ID, nombreMetodo, ex);
                contextoHttp.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                RespuestaModel respuesta = new RespuestaModel();
                respuesta.Identificador = contextoHttp.TraceIdentifier;
                respuesta.Exito = ERROR;
                respuesta.Mensaje = MENSAJE_TOKEN_EXPIRADO;
                await contextoHttp.Response.WriteAsJsonAsync(respuesta);
            }
            catch (Exception ex)
            {
                this._logger.Error(TRACE_ID, nombreMetodo, ex);
                contextoHttp.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                RespuestaModel respuesta = new RespuestaModel();
                respuesta.Identificador = contextoHttp.TraceIdentifier;
                respuesta.Exito = ERROR;
                respuesta.Mensaje = MENSAJE_ERROR_INTERNO;
                await contextoHttp.Response.WriteAsJsonAsync(respuesta);
            }
            finally
            {
                this._logger.Fin(TRACE_ID, nombreMetodo);
            }
        }
    }
}
