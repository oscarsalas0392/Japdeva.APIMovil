using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;
namespace Japdeva.APIMovil.Common.Middlewares.EnviarTraceIdMiddleware
{
    /// <summary>
    /// Middleware para enviar el TraceId en las respuestas HTTP.
    /// </summary>
    public class EnviarTraceIdMiddleware : IEnviarTraceIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<EnviarTraceIdMiddleware> _logger;
        private const string TRACE_ID_HEADER = "TraceId";

        /// <summary>
        /// Inicializa una nueva instancia del middleware EnviarTraceIdMiddlware.
        /// </summary>
        /// <param name="next">El siguiente delegado de solicitud en la canalización.</param>
        /// <param name="logger">El logger para registrar eventos.</param>
        /// <exception cref="ArgumentNullException">Se lanza cuando algún parámetro es null.</exception>
        public EnviarTraceIdMiddleware(RequestDelegate next, ILogger<EnviarTraceIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// <summary>
        /// Procesa la solicitud HTTP y agrega el TraceId a la respuesta.
        /// </summary>
        /// <param name="context">El contexto HTTP de la solicitud.</param>
        /// <exception cref="ArgumentNullException">Se lanza cuando el contexto es null.</exception>
        public async Task InvokeAsync(HttpContext context)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            string traceId = context.TraceIdentifier;
            try
            {
                this._logger.Inicio(nombreMetodo, nombreMetodo);
                if (context is null) throw new ArgumentNullException(nameof(context));
                context.Response.Headers[TRACE_ID_HEADER] = context.TraceIdentifier;
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