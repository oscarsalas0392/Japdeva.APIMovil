using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;

namespace Japdeva.APIMovil.Common.Middlewares
{
    /// <summary>
    /// Middleware para el manejo centralizado de errores en la aplicación.
    /// Captura excepciones no controladas y las transforma en respuestas HTTP apropiadas.
    /// </summary>
    public class ManejoErroresMiddleware : IManejoErroresMiddleware
    {
        private readonly RequestDelegate _siguiente;
        private readonly ILogger<ManejoErroresMiddleware> _logger;
        private const string MENSAJE_TIMEOUT = "La solicitud ha excedido el tiempo de espera.";
        private const string MENSAJE_BAD_REQUEST = "La solicitud contiene datos inválidos.";
        private const string MENSAJE_ERROR_INTERNO = "La solicitud ha fallado.";
        private const int POSICION_INICIO_STREAM = 0;
        private const bool ERROR = false;

        /// <summary>
        /// Inicializa una nueva instancia del middleware de manejo de errores.
        /// </summary>
        /// <param name="siguiente">Siguiente middleware en el pipeline.</param>
        /// <param name="logger">Logger para registrar errores.</param>
        public ManejoErroresMiddleware(RequestDelegate siguiente, ILogger<ManejoErroresMiddleware> logger)
        {
            this._logger = logger;
            this._siguiente = siguiente;
        }

        /// <summary>
        /// Método de invocación del middleware que maneja las excepciones.
        /// </summary>
        /// <param name="contextoHttp">Contexto HTTP de la solicitud.</param>
        public async Task InvokeAsync(HttpContext contextoHttp)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            RespuestaModel respuesta = new RespuestaModel();
            respuesta.Identificador = contextoHttp.TraceIdentifier;
            Stream streamOriginal = contextoHttp.Response.Body;
            try
            {
                this._logger.Inicio(contextoHttp.TraceIdentifier, nombreMetodo);
                using MemoryStream streamTemporal = new MemoryStream();
                contextoHttp.Response.Body = streamTemporal;
                await this._siguiente(contextoHttp);
                streamTemporal.Seek(POSICION_INICIO_STREAM, SeekOrigin.Begin);
                string contenido = await new StreamReader(streamTemporal).ReadToEndAsync();
                contextoHttp.Response.Body = streamOriginal;
                contextoHttp.Response.ContentLength = null;
                await contextoHttp.Response.WriteAsync(contenido);
            }
            catch (TimeoutException ex)
            {
                contextoHttp.Response.StatusCode = (int)HttpStatusCode.RequestTimeout;
                this._logger.Error(contextoHttp.TraceIdentifier, nombreMetodo, ex);
                respuesta.Exito = ERROR;
                respuesta.Mensaje = MENSAJE_TIMEOUT;
                contextoHttp.Response.Body = streamOriginal;
                await contextoHttp.Response.WriteAsJsonAsync(respuesta);
            }
            catch (ArgumentException ex)
            {
                contextoHttp.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                this._logger.Error(contextoHttp.TraceIdentifier, nombreMetodo, ex);
                respuesta.Exito = ERROR;
                respuesta.Mensaje = ex.Message ?? MENSAJE_BAD_REQUEST;
                contextoHttp.Response.Body = streamOriginal;
                await contextoHttp.Response.WriteAsJsonAsync(respuesta);
            }
            catch (Exception ex)
            {
                contextoHttp.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                this._logger.Error(contextoHttp.TraceIdentifier, nombreMetodo, ex);
                respuesta.Exito = ERROR;
                respuesta.Mensaje = MENSAJE_ERROR_INTERNO;
                contextoHttp.Response.Body = streamOriginal;
                await contextoHttp.Response.WriteAsJsonAsync(respuesta);
            }
            finally
            {
                this._logger.Fin(contextoHttp.TraceIdentifier, nombreMetodo);
            }
        }
    }
}
