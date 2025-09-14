using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace Japdeva.APIMovil.Common.Extensions
{

        /// <summary>
        /// Contiene métodos de extensión para configurar Log4Net en un WebApplicationBuilder.
        /// </summary>
        public static class Log4NetExtension
        {
            private const string DIRECTORIO_PADRE = "..";
            private const string PROYECTO_COMMON = "Japdeva.APIMovil.Common";
            private const string ARCHIVO_LOG4NET = "log4net.config";

            /// <summary>
            /// Agrega la configuración de Log4Net al WebApplicationBuilder.
            /// </summary>
            /// <param name="builder">El WebApplicationBuilder al que se agregará Log4Net.</param>
            /// <returns>El mismo WebApplicationBuilder con Log4Net configurado.</returns>
            public static WebApplicationBuilder AgregarLog4Net(this WebApplicationBuilder builder)
            {
                string directorioActual = Directory.GetCurrentDirectory();
                string rutaLog4Net = Path.Combine(directorioActual, DIRECTORIO_PADRE, PROYECTO_COMMON, ARCHIVO_LOG4NET);
                rutaLog4Net = Path.GetFullPath(rutaLog4Net);
                builder.Logging.ClearProviders();
                builder.Logging.AddLog4Net(rutaLog4Net);
                return builder;
            }
        }
}
