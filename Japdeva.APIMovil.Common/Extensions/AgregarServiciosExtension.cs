using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ocelot.DependencyInjection;
using Japdeva.APIMovil.Common.Services;

namespace Japdeva.APIMovil.Common.Extensions
{
    /// <summary>
    /// Proporciona métodos de extensión para agregar servicios a la aplicación.
    /// </summary>
    public static class AgregarServiciosExtension
    {
        private const string DIRECTORIO_PADRE = "..";
        private const string PROYECTO_COMMON = "Japdeva.APIMovil.Common";
        private const string ARCHIVO_LOG4NET = "log4net.config";

        /// <summary>
        /// Agrega los servicios necesarios para los microservicios a la aplicación.
        /// </summary>
        public static WebApplicationBuilder AgregarServiciosMicroservicios(this WebApplicationBuilder builder)
        {
            try
            {
                if (builder is null) throw new ArgumentNullException(nameof(builder));
                string directorioActual = Directory.GetCurrentDirectory();
                string rutaLog4Net = Path.Combine(directorioActual, DIRECTORIO_PADRE, PROYECTO_COMMON, ARCHIVO_LOG4NET);
                rutaLog4Net = Path.GetFullPath(rutaLog4Net);
                builder.Logging.ClearProviders();
                builder.Logging.AddLog4Net(rutaLog4Net);
                builder.Services.AddControllers();
                builder.Services.AddOpenApi();
                builder.Services.AddSingleton<IValidarTokenService, ValidarTokenService>();
                return builder;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Agrega los servicios necesarios para el gateway a la aplicación.
        /// </summary>
        public static WebApplicationBuilder AgregarServiciosGateway(this WebApplicationBuilder builder)
        {
            try
            {
                if (builder is null) throw new ArgumentNullException(nameof(builder));
                
                // Construir la ruta al archivo log4net.config dinámicamente
                string directorioActual = Directory.GetCurrentDirectory();
                string rutaLog4Net = Path.Combine(directorioActual, DIRECTORIO_PADRE, PROYECTO_COMMON, ARCHIVO_LOG4NET);
                rutaLog4Net = Path.GetFullPath(rutaLog4Net);
                
                builder.Logging.ClearProviders();
                builder.Logging.AddLog4Net(rutaLog4Net);
                builder.Services.AddControllers();
                builder.Services.AddOpenApi();
                builder.Services.AddScoped<IGenerarTokenService, GenerarTokenService>();
                builder.Services.AddScoped<IValidarTokenService, ValidarTokenService>();
                builder.Services.AddOcelot(builder.Configuration);
                return builder;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
