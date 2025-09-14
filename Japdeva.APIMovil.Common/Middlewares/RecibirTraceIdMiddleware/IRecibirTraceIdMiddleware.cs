using Microsoft.AspNetCore.Http;

namespace Japdeva.APIMovil.Common.Middlewares.RecibirTraceIdMiddleware
{

    /// <summary>
    /// Interfaz para el middleware de recibir TraceId
    /// </summary>
    public interface IRecibirTraceIdMiddleware
    {
        /// <summary>
        /// Ejecuta el middleware para recibir y establecer el TraceId
        /// </summary>
        /// <param name="context">El contexto de la solicitud HTTP</param>
        /// <returns>Una tarea que representa la operación asíncrona</returns>
        Task InvokeAsync(HttpContext context);
    }
}