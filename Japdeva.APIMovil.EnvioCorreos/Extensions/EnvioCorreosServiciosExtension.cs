using Japdeva.APIMovil.EnvioCorreos.BackgroundServices;
using Japdeva.APIMovil.EnvioCorreos.Services.AgregarCorreoService;
using Japdeva.APIMovil.EnvioCorreos.Services.EnviarCorreoHistoricoService;
using Japdeva.APIMovil.EnvioCorreos.Services.ProcesarCorreosPendientesService;
using Japdeva.APIMovil.EnvioCorreos.Services.SmtpService;

namespace Japdeva.APIMovil.EnvioCorreos.Extensions
{
    /// <summary>
    /// Extensión para registrar los servicios del proyecto EnvioCorreos.
    /// </summary>
    public static class EnvioCorreosServiciosExtension
    {
        /// <summary>
        /// Agrega los servicios del proyecto EnvioCorreos al contenedor de inyección de dependencias.
        /// </summary>
        /// <param name="builder">El builder de la aplicación web.</param>
        /// <returns>El builder configurado con los servicios de EnvioCorreos.</returns>
        public static WebApplicationBuilder AgregarServiciosEnvioCorreos(this WebApplicationBuilder builder)
        {
            try
            {
                if (builder is null) throw new ArgumentNullException(nameof(builder));

                builder.Services.AddSingleton<ISmtpService, SmtpService>();
                builder.Services.AddSingleton<IAgregarCorreoService, AgregarCorreoService>();

                builder.Services.AddScoped<IProcesarCorreosPendientesService, ProcesarCorreosPendientesService>();
                builder.Services.AddScoped<IEnviarCorreoHistoricoService, EnviarCorreoHistoricoService>();

                builder.Services.AddHostedService<EnviarCorreosBackGroundService>();
                builder.Services.AddHostedService<HistoricoCorreosBackGroundService>();

                return builder;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
