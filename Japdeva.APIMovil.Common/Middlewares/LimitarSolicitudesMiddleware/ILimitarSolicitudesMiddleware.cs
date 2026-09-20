using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;

namespace Japdeva.APIMovil.Common.Middlewares
{
    /// <summary>
    /// Interfaz para el middleware de limitación de solicitudes por IP.
    /// </summary>
    public interface ILimitarSolicitudesMiddleware
    {
        /// <summary>
        /// Método de invocación del middleware que aplica el límite de solicitudes.
        /// </summary>
        /// <param name="context">Contexto HTTP de la solicitud.</param>
        /// <returns>Task que representa la operación asíncrona.</returns>
        Task InvokeAsync(HttpContext context);

        /// <summary>
        /// Crea un nuevo limitador de tasa para una IP determinada.
        /// </summary>
        /// <returns>Instancia de <see cref="RateLimiter"/> configurada.</returns>
        RateLimiter CrearLimitador();
    }
}
