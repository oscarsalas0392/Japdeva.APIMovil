using Japdeva.APIMovil.BaseDatos.Services.BaseDatosService;
using Japdeva.APIMovil.BaseDatos.Services.EjecutarScriptsService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Japdeva.APIMovil.BaseDatos.Extensions
{
    /// <summary>
    /// Extensión para registrar los servicios del proyecto BaseDatos.
    /// </summary>
    public static class BaseDatosServiciosExtension
    {
        /// <summary>
        /// Agrega los servicios del proyecto BaseDatos al contenedor de inyección de dependencias.
        /// </summary>
        /// <param name="builder">El builder de la aplicación.</param>
        /// <returns>El builder configurado con los servicios de BaseDatos.</returns>
        public static IHostApplicationBuilder AgregarServiciosBaseDatos(this IHostApplicationBuilder builder)
        {
            try
            {
                if (builder is null) throw new ArgumentNullException(nameof(builder));

                builder.Services.AddSingleton<IBaseDatosService, BaseDatosService>();
                builder.Services.AddSingleton<IEjecutarScriptsService, EjecutarScriptsService>();

                return builder;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
