using Microsoft.AspNetCore.Http;

namespace Japdeva.APIMovil.Common.Middlewares.EnviarTraceIdMiddleware
{
    /// <summary>
    /// Interfaz para el middleware de enviar TraceId
    /// </summary>
    public interface IEnviarTraceIdMiddleware
    {
        /// <summary>
        /// Ejecuta el middleware para enviar el TraceId en las respuestas HTTP
        /// </summary>
        /// <param name="context">El contexto de la solicitud HTTP</param>
        /// <returns>Una tarea que representa la operación asíncrona</returns>
        Task InvokeAsync(HttpContext context);
    }
}