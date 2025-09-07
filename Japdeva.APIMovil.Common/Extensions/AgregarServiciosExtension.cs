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
        private const string LOG4NET_CONFIG = "log4net.config";

        /// <summary>
        /// Agrega los servicios necesarios para los microservicios a la aplicación.
        /// </summary>
        public static void AgregarServiciosMicroservicios(this WebApplicationBuilder builder)
        {
            try
            {
                if (builder is null) throw new ArgumentNullException(nameof(builder));
                builder.Logging.ClearProviders();
                builder.Logging.AddLog4Net(LOG4NET_CONFIG);
                builder.Services.AddControllers();
                builder.Services.AddOpenApi();
                builder.Services.AddScoped<IValidarTokenService, ValidarTokenService>();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Agrega los servicios necesarios para el gateway a la aplicación.
        /// </summary>
        public static void AgregarServiciosGateway(this WebApplicationBuilder builder)
        {
            try
            {
                if (builder is null) throw new ArgumentNullException(nameof(builder));
                builder.Logging.ClearProviders();
                builder.Logging.AddLog4Net(LOG4NET_CONFIG);
                builder.Services.AddControllers();
                builder.Services.AddOpenApi();
                builder.Services.AddScoped<IGenerarTokenService, GenerarTokenService>();
                builder.Services.AddScoped<IValidarTokenService, ValidarTokenService>();
                builder.Services.AddOcelot(builder.Configuration);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
