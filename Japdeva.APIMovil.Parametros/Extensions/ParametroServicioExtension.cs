using Japdeva.APIMovil.Parametros.BackgroundServices;
using Japdeva.APIMovil.Parametros.BackGroundServices;
using Japdeva.APIMovil.Parametros.Services.MenuCacheService;
using Japdeva.APIMovil.Parametros.Services.MenuPerfilCacheService;
using Japdeva.APIMovil.Parametros.Services.ObtenerMenuPorPerfilService;
using Japdeva.APIMovil.Parametros.Services.ObtenerOpcionPantallaPorPerfilService;
using Japdeva.APIMovil.Parametros.Services.OpcionPantallaCacheService;
using Japdeva.APIMovil.Parametros.Services.OpcionPantallaPerfilCacheService;
using Japdeva.APIMovil.Parametros.Services.ObtenerParametroService;
using Japdeva.APIMovil.Parametros.Services.ObtenerParametrosService;
using Japdeva.APIMovil.Parametros.Services.ObtenerPlantillaNuevoReclamoService;
using Japdeva.APIMovil.Parametros.Services.PantallaCacheService;
using Japdeva.APIMovil.Parametros.Services.ParametroCacheService;
using Japdeva.APIMovil.Parametros.Services.PlantillaCorreoCacheService;

namespace Japdeva.APIMovil.Parametros.Extensions
{
    /// <summary>
    /// Proporciona métodos de extensión para registrar los servicios relacionados con parámetros en el contenedor de dependencias.
    /// </summary>
    public static class ParametroServicioExtension
    {
        /// <summary>
        /// Agrega los servicios necesarios para la gestión de parámetros a la colección de servicios del <see cref="WebApplicationBuilder"/>.
        /// </summary>
        /// <param name="builder">El constructor de la aplicación web al que se agregarán los servicios.</param>
        /// <returns>El mismo <see cref="WebApplicationBuilder"/> para permitir la encadenación de métodos.</returns>
        public static WebApplicationBuilder AgregarServiciosParametros(this WebApplicationBuilder builder)
        {
            try
            {
                if (builder is null) throw new ArgumentNullException(nameof(builder));

                // Registrar servicios de caché como Singleton
                builder.Services.AddSingleton<IPantallaCacheService, PantallaCacheService>();
                builder.Services.AddSingleton<IMenuCacheService, MenuCacheService>();
                builder.Services.AddSingleton<IMenuPerfilCacheService, MenuPerfilCacheService>();
                builder.Services.AddSingleton<IParametroCacheService, ParametroCacheService>();
                builder.Services.AddSingleton<IPlantillaCorreoCacheService, PlantillaCorreoCacheService>();
                builder.Services.AddSingleton<IOpcionPantallaCacheService, OpcionPantallaCacheService>();
                builder.Services.AddSingleton<IOpcionPantallaPerfilCacheService, OpcionPantallaPerfilCacheService>();

                // Registrar servicios de negocio
                builder.Services.AddScoped<IObtenerParametroService, ObtenerParametroService>();
                builder.Services.AddScoped<IObtenerParametrosService, ObtenerParametrosService>();
                builder.Services.AddScoped<IObtenerMenuPorPerfilService, ObtenerMenuPorPerfilService>();
                builder.Services.AddScoped<IObtenerPlantillaNuevoReclamoService, ObtenerPlantillaNuevoReclamoService>();
                builder.Services.AddScoped<IObtenerOpcionPantallaPorPerfilService, ObtenerOpcionPantallaPorPerfilService>();

                // Registrar servicio de fondo para actualización de cachés
                builder.Services.AddHostedService<ParametrosBackGroundService>();
                builder.Services.AddHostedService<ObtenerPlantillaBackGroundService>();
                return builder;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
