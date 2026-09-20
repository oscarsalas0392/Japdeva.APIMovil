using Microsoft.AspNetCore.Http;

namespace Japdeva.APIMovil.Common.Middlewares.ManejoErroresGatewayMiddleware
{
    /// <summary>
    /// Interfaz para el middleware de manejo centralizado de errores en el Gateway.
    /// </summary>
    public interface IManejoErroresGatewayMiddleware
    {
        /// <summary>
        /// Método de invocación del middleware que maneja las excepciones del Gateway.
        /// </summary>
        /// <param name="contextoHttp">Contexto HTTP de la solicitud.</param>
        /// <returns>Task que representa la operación asíncrona.</returns>
        Task InvokeAsync(HttpContext contextoHttp);
    }
}
