using Microsoft.AspNetCore.Http;

namespace Japdeva.APIMovil.Common.Middlewares
{
    /// <summary>
    /// Interfaz para el middleware de creación de tokens de autenticación.
    /// Define el contrato para generar y agregar tokens JWT a las solicitudes HTTP.
    /// </summary>
    public interface ICrearTokenMiddleware
    {
        /// <summary>
        /// Método de invocación del middleware que crea y agrega el token.
        /// </summary>
        /// <param name="context">Contexto HTTP de la solicitud.</param>
        /// <returns>Task que representa la operación asíncrona.</returns>
        Task InvokeAsync(HttpContext context);
    }
}
