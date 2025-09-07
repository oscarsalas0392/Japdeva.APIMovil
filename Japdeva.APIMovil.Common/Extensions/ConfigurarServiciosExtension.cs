using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Ocelot.Middleware;

namespace Japdeva.APIMovil.Common.Extensions
{
    /// <summary>
    /// Clase estática que proporciona métodos de extensión para configurar los servicios en la API de Japdeva.
    /// </summary>
    public static class ConfigurarServiciosExtension
    {
        /// <summary>
        /// Configura los servicios para los microservicios de la aplicación.
        /// </summary>
        /// <param name="app">Instancia de la aplicación web.</param>
        public static void ConfigurarServiciosMicroservicios(this WebApplication app)
        {
            try
            {
                if (app is null) throw new ArgumentNullException(nameof(app));
                if (app.Environment.IsDevelopment())
                {
                    app.MapOpenApi();
                }
                app.UseHttpsRedirection();
                app.UseAuthorization();
                app.MapControllers();
                app.Run();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Configura los servicios para el gateway utilizando Ocelot.
        /// </summary>
        /// <param name="app">Instancia de la aplicación web.</param>
        public static async Task ConfigurarServiciosGatewayAsync(this WebApplication app)
        {
            try
            {
                if (app is null) throw new ArgumentNullException(nameof(app));
                if (app.Environment.IsDevelopment())
                {
                    app.MapOpenApi();
                }
                app.UseHttpsRedirection();
                app.UseAuthorization();
                app.MapControllers();
                await app.UseOcelot();
                app.Run();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
