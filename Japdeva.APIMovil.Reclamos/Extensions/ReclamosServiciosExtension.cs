using Japdeva.APIMovil.Reclamos.BackgroundServices;
using Japdeva.APIMovil.Reclamos.Services.AgregarDocumentoInternoService;
using Japdeva.APIMovil.Reclamos.Services.AgregarDocumentoUsuarioService;
using Japdeva.APIMovil.Reclamos.Services.AgregarReclamoDetalleService;
using Japdeva.APIMovil.Reclamos.Services.AgregarReclamoService;
using Japdeva.APIMovil.Reclamos.Services.EditarDepartamentoReclamoService;
using Japdeva.APIMovil.Reclamos.Services.EditarReclamoDetalleService;
using Japdeva.APIMovil.Reclamos.Services.EditarReclamoService;
using Japdeva.APIMovil.Reclamos.Services.EliminarDocumentoInternoService;
using Japdeva.APIMovil.Reclamos.Services.EnvioHistoricoDetalleReclamoService;
using Japdeva.APIMovil.Reclamos.Services.EnvioHistoricoDocumentoInternoService;
using Japdeva.APIMovil.Reclamos.Services.EnvioHistoricoDocumentoUsuarioService;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoCacheService;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoOrdenProcesoCacheService;
using Japdeva.APIMovil.Reclamos.Services.EstadoReclamoCacheService;
using Japdeva.APIMovil.Reclamos.Services.ListaRespuestaReclamoService;
using Japdeva.APIMovil.Reclamos.Services.NivelProcesoCacheService;
using Japdeva.APIMovil.Reclamos.Services.NotificarDepartamentoService;
using Japdeva.APIMovil.Reclamos.Services.NotificarResolucionUsuarioService;
using Japdeva.APIMovil.Reclamos.Services.NotificarUsuarioReclamoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerDetalleReclamoHistoricoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerDetalleReclamoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerDocumentoInternoHistoricoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerDocumentoInternoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerDocumentoUsuarioHistoricoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerDocumentoUsuarioService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerEstadoDetalleReclamoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerOrdenNivelProcesoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerReclamoPorDepartamentoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerReclamosPorFechaIngresoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerReclamosPorUsuarioService;
using Japdeva.APIMovil.Reclamos.Services.OrdenNivelProcesoCacheService;
using Japdeva.APIMovil.Reclamos.Services.ValidarEnvioReclamoHistoricoService;
using Japdeva.APIMovil.Reclamos.Services.ValidarEstadoDetalleReclamoService;

namespace Japdeva.APIMovil.Reclamos.Extensions
{
    /// <summary>
    /// Clase de extensión para el registro de todos los servicios del módulo de Reclamos.
    /// Proporciona un método centralizado para configurar la inyección de dependencias
    /// de todos los servicios especializados del microservicio Reclamos.
    /// </summary>
    public static class ReclamosServiciosExtension
    {
        /// <summary>
        /// Agrega todos los servicios del módulo de Reclamos al contenedor de inyección de dependencias.
        /// Registra servicios de CRUD, cache, históricos, validaciones y background services
        /// utilizando el patrón Singleton para optimizar el performance y la gestión de memoria.
        /// </summary>
        /// <param name="builder">El constructor de la aplicación web para configurar los servicios.</param>
        /// <returns>El mismo constructor de aplicación web para permitir encadenamiento de métodos.</returns>
        /// <exception cref="ArgumentNullException">Se lanza cuando el builder es null.</exception>
        /// <exception cref="InvalidOperationException">Se lanza cuando ocurre un error durante el registro de servicios.</exception>
        public static WebApplicationBuilder AgregarServiciosReclamos(this WebApplicationBuilder builder)
        {

            try
            {

                builder.Services.AddSingleton<IAgregarDocumentoInternoService, AgregarDocumentoInternoService>();
                builder.Services.AddSingleton<IAgregarDocumentoUsuarioService, AgregarDocumentoUsuarioService>();
                builder.Services.AddSingleton<IAgregarReclamoDetalleService, AgregarReclamoDetalleService>();
                builder.Services.AddSingleton<IAgregarReclamoService, AgregarReclamoService>();
                builder.Services.AddSingleton<IEditarReclamoService, EditarReclamoService>();
                builder.Services.AddSingleton<IEditarReclamoDetalleService, EditarReclamoDetalleService>();
                builder.Services.AddSingleton<IEliminarDocumentoInternoService, EliminarDocumentoInternoService>();
                builder.Services.AddSingleton<IObtenerDetalleReclamoHistoricoService, ObtenerDetalleReclamoHistoricoService>();
                builder.Services.AddSingleton<IObtenerDetalleReclamoService, ObtenerDetalleReclamoService>();
                builder.Services.AddSingleton<IObtenerDocumentoInternoHistoricoService, ObtenerDocumentoInternoHistoricoService>();
                builder.Services.AddSingleton<IObtenerDocumentoInternoService, ObtenerDocumentoInternoService>();
                builder.Services.AddSingleton<IObtenerDocumentoUsuarioHistoricoService, ObtenerDocumentoUsuarioHistoricoService>();
                builder.Services.AddSingleton<IObtenerDocumentoUsuarioService, ObtenerDocumentoUsuarioService>();
                builder.Services.AddSingleton<IObtenerEstadoDetalleReclamoService, ObtenerEstadoDetalleReclamoService>();
                builder.Services.AddSingleton<IObtenerOrdenNivelProcesoService, ObtenerOrdenNivelProcesoService>();
                builder.Services.AddSingleton<IObtenerReclamoPorDepartamentoService, ObtenerReclamoPorDepartamentoService>();
                builder.Services.AddSingleton<IObtenerReclamosPorFechaIngresoService, ObtenerReclamosPorFechaIngresoService>();
                builder.Services.AddSingleton<IObtenerReclamosPorUsuarioService, ObtenerReclamosPorUsuarioService>();
                builder.Services.AddSingleton<IListaRespuestaReclamoService, ListaRespuestaReclamoService>();
                builder.Services.AddSingleton<IEstadoDetalleReclamoCacheService, EstadoDetalleReclamoCacheService>();
                builder.Services.AddSingleton<IEstadoDetalleReclamoOrdenProcesoCacheService, EstadoDetalleReclamoOrdenProcesoCacheService>();
                builder.Services.AddSingleton<IEstadoReclamoCacheService, EstadoReclamoCacheService>();
                builder.Services.AddSingleton<INivelProcesoCacheService, NivelProcesoCacheService>();
                builder.Services.AddSingleton<IOrdenNivelProcesoCacheService, OrdenNivelProcesoCacheService>();           
              
                builder.Services.AddSingleton<IValidarEstadoDetalleReclamoService, ValidarEstadoDetalleReclamoService>();
                builder.Services.AddSingleton<IEditarDepartamentoReclamoService, EditarDepartamentoReclamoService>();
                builder.Services.AddSingleton<INotificarDepartamentoService, NotificarDepartamentoService>();
                builder.Services.AddSingleton<INotificarResolucionUsuarioService, NotificarResolucionUsuarioService>();
                builder.Services.AddSingleton<INotificarUsuarioReclamoService, NotificarUsuarioReclamoService>();

                builder.Services.AddScoped<IEnvioHistoricoDetalleReclamoService, EnvioHistoricoDetalleReclamoService>();
                builder.Services.AddScoped<IEnvioHistoricoDocumentoInternoService, EnvioHistoricoDocumentoInternoService>();
                builder.Services.AddScoped<IEnvioHistoricoDocumentoUsuarioService, EnvioHistoricoDocumentoUsuarioService>();
                builder.Services.AddScoped<IValidarEnvioReclamoHistoricoService, ValidarEnvioReclamoHistoricoService>();

                builder.Services.AddHostedService<HistoricoBackGroundService>();
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
