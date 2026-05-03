using Microsoft.AspNetCore.Http;

namespace Japdeva.APIMovil.Common.Middlewares
{
    /// <summary>
    /// Interfaz para el middleware de gestión de token en la respuesta.
    /// Define el contrato para generar o refrescar el token del cliente en las respuestas HTTP.
    /// </summary>
    public interface IGestionarTokenRespuestaMiddleware
    {
        /// <summary>
        /// Método de invocación del middleware que gestiona el token en la respuesta.
        /// </summary>
        /// <param name="context">Contexto HTTP de la solicitud.</param>
        /// <returns>Task que representa la operación asíncrona.</returns>
        Task InvokeAsync(HttpContext context);

        /// <summary>
        /// Genera o refresca el token del cliente según el contexto de la solicitud.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="esRutaAutenticar">Indica si la ruta es la de autenticación.</param>
        /// <param name="tokenEntrada">Token recibido en la solicitud.</param>
        /// <param name="statusCode">Código de estado de la respuesta.</param>
        /// <returns>Nuevo token o cadena vacía si no aplica.</returns>
        string ObtenerNuevoToken(string traceId, bool esRutaAutenticar, string tokenEntrada, int statusCode);

        /// <summary>
        /// Valida el token de entrada y genera un token refrescado con el mismo rol.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="tokenEntrada">Token vigente del cliente.</param>
        /// <returns>Nuevo token refrescado.</returns>
        string RefrescarToken(string traceId, string tokenEntrada);

    }
}
