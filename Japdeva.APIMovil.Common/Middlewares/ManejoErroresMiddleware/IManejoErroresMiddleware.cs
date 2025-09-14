using Microsoft.AspNetCore.Http;

namespace Japdeva.APIMovil.Common.Middlewares
{
    /// <summary>
    /// Interfaz para el middleware de manejo centralizado de errores en la aplicación.
    /// Define el contrato para capturar excepciones no controladas y transformarlas en respuestas HTTP apropiadas.
    /// </summary>
    public interface IManejoErroresMiddleware
    {
        /// <summary>
        /// Método de invocación del middleware que maneja las excepciones.
        /// </summary>
        /// <param name="contextoHttp">Contexto HTTP de la solicitud.</param>
        /// <returns>Task que representa la operación asíncrona.</returns>
        Task InvokeAsync(HttpContext contextoHttp);
    }
}
