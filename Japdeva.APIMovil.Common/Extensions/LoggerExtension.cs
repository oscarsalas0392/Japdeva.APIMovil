using Microsoft.Extensions.Logging;

namespace Japdeva.APIMovil.Common.Extensions
{
    /// <summary>
    /// Proporciona métodos de extensión para ILogger que facilitan el registro estandarizado
    /// de eventos con identificadores de rastreo y nombres de métodos.
    /// </summary>
    public static class LoggerExtension
    {
        private const string IDENTIFICADOR = "Identificador:";
        private const string INICIO_METODO = "inicio del método:";
        private const string FIN_METODO = "fin del método:";
        private const string NOMBRE_METODO = "nombre del método:";

        /// <summary>
        /// Registra un mensaje de información indicando el inicio de un método.
        /// </summary>
        /// <param name="logger">La instancia del logger donde se registrará el mensaje.</param>
        /// <param name="traceId">Identificador único de rastreo para seguimiento de la operación.</param>
        /// <param name="nombreMetodo">Nombre del método que está iniciando su ejecución.</param>
        public static void Inicio(this ILogger logger, string traceId, string nombreMetodo)
        {
            logger.LogInformation($"{IDENTIFICADOR} {traceId}, {INICIO_METODO} {nombreMetodo}");
        }

        /// <summary>
        /// Registra un error con información contextual del método donde ocurrió la excepción.
        /// </summary>
        /// <param name="logger">La instancia del logger donde se registrará el error.</param>
        /// <param name="traceId">Identificador único de rastreo para seguimiento de la operación.</param>
        /// <param name="nombreMetodo">Nombre del método donde ocurrió el error.</param>
        /// <param name="ex">La excepción que se produjo y debe ser registrada.</param>
        public static void Error(this ILogger logger, string traceId, string nombreMetodo, Exception ex)
        {
            logger.LogError($"{IDENTIFICADOR} {traceId}, {NOMBRE_METODO} {nombreMetodo}", ex);
        }

        /// <summary>
        /// Registra un mensaje de información indicando la finalización exitosa de un método.
        /// </summary>
        /// <param name="logger">La instancia del logger donde se registrará el mensaje.</param>
        /// <param name="traceId">Identificador único de rastreo para seguimiento de la operación.</param>
        /// <param name="nombreMetodo">Nombre del método que finalizó su ejecución.</param>
        public static void Fin(this ILogger logger, string traceId, string nombreMetodo)
        {
            logger.LogInformation($"{IDENTIFICADOR} {traceId}, {FIN_METODO} {nombreMetodo}");
        }
    }
}
