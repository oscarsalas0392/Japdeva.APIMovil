using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;

namespace Japdeva.APIMovil.Common.Middlewares.RecibirTraceIdMiddleware
{
    /// <summary>
    /// Middleware para recibir y establecer el TraceId en el contexto de la solicitud HTTP.
    /// </summary>
    public class RecibirTraceIdMiddleware : IRecibirTraceIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RecibirTraceIdMiddleware> _logger;
        private const string TRACE_ID_HEADER = "TraceId";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="RecibirTraceIdMiddleware"/>.
        /// </summary>
        /// <param name="next">El delegado de la siguiente solicitud en la canalización.</param>
        /// <param name="logger">El registrador para la clase <see cref="RecibirTraceIdMiddleware"/>.</param>
        public RecibirTraceIdMiddleware(RequestDelegate next, ILogger<RecibirTraceIdMiddleware> logger)
        {
            this._next = next;
            this._logger = logger;
        }   

        /// <summary>
        /// Middleware para recibir y establecer el TraceId en el contexto de la solicitud HTTP.
        /// </summary>
        /// <param name="context">El contexto de la solicitud HTTP.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            string traceId = context.TraceIdentifier;
            try
            {
                this._logger.Inicio(nombreMetodo, nombreMetodo);
                if (context is null) throw new ArgumentNullException(nameof(context));
                if (context.Request.Headers.TryGetValue(TRACE_ID_HEADER, out var traceIdHeader))
                {
                    context.TraceIdentifier = traceIdHeader.ToString();
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