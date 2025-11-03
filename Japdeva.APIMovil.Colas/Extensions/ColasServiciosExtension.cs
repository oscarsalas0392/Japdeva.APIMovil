using Japdeva.APIMovil.Colas.BackgroundServices;
using Japdeva.APIMovil.Colas.Repositories;
using Japdeva.APIMovil.Colas.Repositories.MensajesColaRepository;

namespace Japdeva.APIMovil.Colas.Extensions
{
    /// <summary>
    /// Extensión para registrar los servicios específicos del proyecto Colas.
    /// </summary>
    public static class ColasServiciosExtension
    {
        /// <summary>
        /// Agrega los servicios específicos del proyecto Colas al contenedor de inyección de dependencias.
        /// </summary>
        /// <param name="builder">El builder de la aplicación web.</param>
        /// <returns>El builder configurado con los servicios de Colas.</returns>
        public static WebApplicationBuilder AgregarServiciosColas(this WebApplicationBuilder builder)
        {
            try
            {
                if (builder is null) throw new ArgumentNullException(nameof(builder));

                // Registrar repositorios específicos de Colas
                builder.Services.AddScoped<IMensajesColaRepository, MensajesColaRepository>();

                // Registrar background services
                builder.Services.AddHostedService<MensajesBackgroundService>();
                builder.Services.AddHostedService<MensajesHistoricoBackgroundService>();
                builder.Services.AddHostedService<ColasBackgroundService>();
                builder.Services.AddHostedService<ParametrosBackGroundService>();

                return builder;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}