using Japdeva.APIMovil.Colas.BackgroundServices;
using Japdeva.APIMovil.Colas.Interceptors;
using Japdeva.APIMovil.Colas.Repositories.MensajesColaRepository;
using Japdeva.APIMovil.Colas.Services.ActualizarMensajeEnProcesoService;
using Japdeva.APIMovil.Colas.Services.ActualizarMensajeExitosoService;
using Japdeva.APIMovil.Colas.Services.ActualizarMensajeExpiradoService;
using Japdeva.APIMovil.Colas.Services.ActualizarMensajeFallidoService;
using Japdeva.APIMovil.Colas.Services.ColaGrpcService;
using Japdeva.APIMovil.Colas.Services.ColaService;
using Japdeva.APIMovil.Colas.Services.EnviarMensajeService;
using Japdeva.APIMovil.Colas.Services.EstadoMensajeService;
using Japdeva.APIMovil.Colas.Services.GuardarMensajesHistoricoService;
using Japdeva.APIMovil.Colas.Services.MensajeColaService;
using Japdeva.APIMovil.Colas.Services.ObtenerMensajePorIdRpcService;
using Japdeva.APIMovil.Colas.Services.ObtenerMensajesPorColaService;
using Japdeva.APIMovil.Colas.Services.PrioridadService;
using Japdeva.APIMovil.Colas.Services.SuscripcionColaService;
using Japdeva.APIMovil.Common.Extensions;

namespace Japdeva.APIMovil.Colas.Extensions
{
    /// <summary>
    /// Extensión para registrar los servicios específicos del proyecto Colas.
    /// </summary>
    public static class ColasServiciosExtension
    {
        private const string TRACE_ID_STARTUP = "startup";
        private const string MENSAJE_WARMUP_FALLIDO = "Warm-up del caché falló al iniciar. El BackgroundService recargará el caché automáticamente.";
        private const string NOMBRE_METODO_WARMUP = nameof(EjecutarWarmUpAsync);
        private const string FORMATO_LOG_INICIO = "Inicio: {Metodo} - TraceId: {TraceId}";
        private const string FORMATO_LOG_FIN = "Fin: {Metodo} - TraceId: {TraceId}";

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

                builder.Services.AddGrpc(options =>
                {
                    options.Interceptors.Add<TokenGrpcInterceptor>();
                });
                builder.Services.AddSingleton<TokenGrpcInterceptor>();

                builder.Services.AddScoped<IMensajesColaRepository, MensajesColaRepository>();

                builder.Services.AddSingleton<ISuscripcionColaService, SuscripcionColaService>();
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

        /// <summary>
        /// Configura el pipeline HTTP del microservicio Colas.
        /// Ejecuta el warm-up del caché antes de aceptar peticiones gRPC.
        /// </summary>
        /// <param name="app">Instancia de la aplicación web.</param>
        public static async Task ConfigurarServiciosColasAsync(this WebApplication app)
        {
            try
            {
                if (app is null) throw new ArgumentNullException(nameof(app));

                await EjecutarWarmUpAsync(app);

                app.MapGrpcService<ColaGrpcService>();
                app.ConfigurarServiciosMicroservicios();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Carga el caché de mensajes pendientes antes de que el servicio empiece a aceptar peticiones.
        /// Si falla (por ejemplo BD no disponible), el error se registra y el BackgroundService lo reintentará.
        /// </summary>
        /// <param name="app">Instancia de la aplicación web.</param>
        public static async Task EjecutarWarmUpAsync(WebApplication app)
        {
            try
            {
                app.Logger.LogInformation(FORMATO_LOG_INICIO, NOMBRE_METODO_WARMUP, TRACE_ID_STARTUP);
                IMensajeColaService mensajeColaService = app.Services.GetRequiredService<IMensajeColaService>();
                await mensajeColaService.LlenarCacheMensajesAsync(TRACE_ID_STARTUP);
            }
            catch (Exception warmUpEx)
            {
                app.Logger.LogWarning(warmUpEx, MENSAJE_WARMUP_FALLIDO);
            }
            finally
            {
                app.Logger.LogInformation(FORMATO_LOG_FIN, NOMBRE_METODO_WARMUP, TRACE_ID_STARTUP);
            }
        }
    }
}
