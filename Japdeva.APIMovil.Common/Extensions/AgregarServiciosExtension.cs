using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.DependencyInjection;
using Japdeva.APIMovil.Common.Services;
using Japdeva.APIMovil.Common.Services.EncriptarService;
using Japdeva.APIMovil.Common.Services.DesencriptarService;
using Japdeva.APIMovil.Common.Services.EncriptarHelperService;

namespace Japdeva.APIMovil.Common.Extensions
{
    /// <summary>
    /// Proporciona métodos de extensión para agregar servicios a la aplicación.
    /// </summary>
    public static class AgregarServiciosExtension
    {
        /// <summary>
        /// Agrega los servicios necesarios para los microservicios a la aplicación.
        /// </summary>
        public static WebApplicationBuilder AgregarServiciosMicroservicios(this WebApplicationBuilder builder)
        {
            try
            {
                if (builder is null) throw new ArgumentNullException(nameof(builder));
                builder.AgregarLog4Net();
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
                builder.AgregarLog4Net();
                builder.Services.AddControllers();
                builder.Services.AddOpenApi();
                builder.Services.AddSingleton<IGenerarTokenService, GenerarTokenService>();
                builder.Services.AddSingleton<IValidarTokenService, ValidarTokenService>();
                builder.Services.AddSingleton<IEncriptarService, EncriptarService>();
                builder.Services.AddSingleton<IDesencriptarService, DesencriptarService>();
                builder.Services.AddSingleton<IEncriptarHelperService, EncriptarHelperService>();
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
