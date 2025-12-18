using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.DependencyInjection;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Common.Repositories.EliminarRepository;
using Japdeva.APIMovil.Common.Repositories.GeneralRepository;
using Japdeva.APIMovil.Common.Services;
using Japdeva.APIMovil.Common.Services.DesencriptarService;
using Japdeva.APIMovil.Common.Services.EncriptarHelperService;
using Japdeva.APIMovil.Common.Services.EncriptarService;

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
        /// <param name="builder">El builder de la aplicación web</param>
        /// <returns>El builder de la aplicación web configurado</returns>
        public static WebApplicationBuilder AgregarServiciosMicroservicios(this WebApplicationBuilder builder)
        {
            try
            {
                if (builder is null) throw new ArgumentNullException(nameof(builder));
                builder.AgregarLog4Net();
                builder.Services.AddControllers();
                builder.Services.AddOpenApi();
                builder.Services.AddScoped<IActualizarRepository, ActualizarRepository>();
                builder.Services.AddScoped<IAgregarRepository, AgregarRepository>();
                builder.Services.AddScoped<IConsultarListaRepository, ConsultarListaRepository>();
                builder.Services.AddScoped<IConsultarRepository, ConsultarRepository>();
                builder.Services.AddScoped<IEliminarRepository, EliminarRepository>();
                builder.Services.AddScoped<IGeneralRepository, GeneralRepository>();
                builder.AddJwtAuthentication();

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
        /// <param name="builder">El builder de la aplicación web</param>
        /// <returns>El builder de la aplicación web configurado</returns>
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
