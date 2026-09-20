using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Services.EncriptarService;

namespace Japdeva.APIMovil.Common.Middlewares.EncriptarRespuestaMiddleware
{
    /// <summary>
    /// Middleware para encriptar la respuesta HTTP.
    /// </summary>
    public class EncriptarRespuestaMiddleware : IEncriptarRespuestaMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<EncriptarRespuestaMiddleware> _logger;
        private readonly IEncriptarService _encriptarService;
        private const string CLAVE_ENCRIPTACION_VARIABLE = "CLAVE_ENCRIPTACION_RESPUESTA";
        private const string ERROR_CLAVE_NO_CONFIGURADA = "La clave de encriptación no está configurada.";
        private const long POSICION_INICIAL_STREAM = 0;
        private const long POSICION_SEEK_STREAM = 0;
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="EncriptarRespuestaMiddleware"/>.
        /// </summary>
        /// <param name="next">Delegado para la siguiente operación en la canalización HTTP.</param>
        /// <param name="logger">Instancia de logger para registrar información.</param>
        public EncriptarRespuestaMiddleware(RequestDelegate next, ILogger<EncriptarRespuestaMiddleware> logger, IEncriptarService encriptarService)
        {
            this._next = next;
            this._logger = logger;
            this._encriptarService = encriptarService;
        }

        /// <summary>
        /// Invoca el middleware de forma asíncrona.
        /// </summary>
        /// <param name="context">Contexto HTTP actual.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            string traceId = context.TraceIdentifier;
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                Stream streamOriginal = context.Response.Body;    
                using MemoryStream streamTemporal = new MemoryStream();
                context.Response.Body = streamTemporal;
                await this._next(context);
                streamTemporal.Seek(POSICION_INICIAL_STREAM, SeekOrigin.Begin);
                streamTemporal.Seek(POSICION_SEEK_STREAM, SeekOrigin.Begin);
                string contenidoRespuesta = await new StreamReader(streamTemporal).ReadToEndAsync();
                if (!string.IsNullOrWhiteSpace(contenidoRespuesta))
                {
                    string? claveEncriptacion = Environment.GetEnvironmentVariable(CLAVE_ENCRIPTACION_VARIABLE);
                    if (string.IsNullOrWhiteSpace(claveEncriptacion)) throw new InvalidOperationException(ERROR_CLAVE_NO_CONFIGURADA);
                    string contenidoEncriptado = this._encriptarService.EncriptarConClave(traceId, contenidoRespuesta, claveEncriptacion);
                    context.Response.Body = streamOriginal;
                    await context.Response.WriteAsync(contenidoEncriptado);
                }   
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