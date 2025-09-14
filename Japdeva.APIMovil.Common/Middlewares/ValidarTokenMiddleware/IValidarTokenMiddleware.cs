using Microsoft.AspNetCore.Http;

namespace Japdeva.APIMovil.Common.Middlewares
{
    /// <summary>
    /// Interfaz para el middleware de validación de tokens de autenticación.
    /// Define el contrato para validar tokens en las solicitudes HTTP.
    /// </summary>
    public interface IValidarTokenMiddleware
    {
        /// <summary>
        /// Método de invocación del middleware que valida el token.
        /// </summary>
        /// <param name="context">Contexto HTTP de la solicitud.</param>
        /// <returns>Task que representa la operación asíncrona.</returns>
        Task InvokeAsync(HttpContext context);
    }
}
