using Microsoft.AspNetCore.Http;

namespace Japdeva.APIMovil.Common.Middlewares.EncriptarRespuestaMiddleware
{
    /// <summary>
    /// Interfaz para el middleware de encriptación de respuesta.
    /// </summary>
    public interface IEncriptarRespuestaMiddleware
    {
        /// <summary>
        /// Invoca el middleware de forma asíncrona.
        /// </summary>
        /// <param name="context">Contexto HTTP actual.</param>
        /// <returns>Tarea que representa la operación asíncrona.</returns>
        Task InvokeAsync(HttpContext context);
    }
}