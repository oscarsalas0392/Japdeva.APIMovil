using Japdeva.APIMovil.Colas.BackgroundServices;
using Japdeva.APIMovil.Colas.Repositories.MensajesColaRepository;
using Japdeva.APIMovil.Colas.Services.ActualizarMensajeEnProcesoService;
using Japdeva.APIMovil.Colas.Services.ActualizarMensajeExitosoService;
using Japdeva.APIMovil.Colas.Services.ActualizarMensajeExpiradoService;
using Japdeva.APIMovil.Colas.Services.ActualizarMensajeFallidoService;
using Japdeva.APIMovil.Colas.Services.ColaService;
using Japdeva.APIMovil.Colas.Services.EnviarMensajeService;
using Japdeva.APIMovil.Colas.Services.EstadoMensajeService;
using Japdeva.APIMovil.Colas.Services.GuardarMensajesHistoricoService;
using Japdeva.APIMovil.Colas.Services.MensajeColaService;
using Japdeva.APIMovil.Colas.Services.ObtenerMensajePorIdRpcService;
using Japdeva.APIMovil.Colas.Services.ObtenerMensajesPorColaService;
using Japdeva.APIMovil.Colas.Services.PrioridadService;

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

                builder.Services.AddSingleton<IActualizarMensajeEnProcesoService, ActualizarMensajeEnProcesoService>();
                builder.Services.AddSingleton<IActualizarMensajeExitosoService, ActualizarMensajeExitosoService>();
                builder.Services.AddSingleton<IActualizarMensajeExpiradoService, ActualizarMensajeExpiradoService>();
                builder.Services.AddSingleton<IActualizarMensajeFallidoService, ActualizarMensajeFallidoService>();
                builder.Services.AddSingleton<IColaService, ColaService>();
                builder.Services.AddSingleton<IEnviarMensajeService, EnviarMensajeService>();
                builder.Services.AddSingleton<IEstadoMensajeService, EstadoMensajeService>();
                builder.Services.AddSingleton<IGuardarMensajesHistoricoService, GuardarMensajesHistoricoService>();
                builder.Services.AddSingleton<IMensajeColaService, MensajeColaService>();
                builder.Services.AddSingleton<IObtenerMensajesPorColaService, ObtenerMensajesPorColaService>();
                builder.Services.AddSingleton<IObtenerMensajePorIdRpcService, ObtenerMensajePorIdRpcService>();
                builder.Services.AddSingleton<IPrioridadService, PrioridadService>();

                // Registrar background services
                builder.Services.AddHostedService<MensajesBackgroundService>();
                builder.Services.AddHostedService<MensajesHistoricoBackgroundService>();
                builder.Services.AddHostedService<ColasBackgroundService>();
                builder.Services.AddHostedService<ParametrosBackGroundService>();
                builder.Services.AddHostedService<MensajeColaExpiradosService>();

                return builder;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}