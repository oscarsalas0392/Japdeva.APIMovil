
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Ocelot.Middleware;
using Japdeva.APIMovil.Common.Middlewares;
using Japdeva.APIMovil.Common.Middlewares.EncriptarRespuestaMiddleware;
using Japdeva.APIMovil.Common.Middlewares.EnviarTraceIdMiddleware;
using Japdeva.APIMovil.Common.Middlewares.RecibirTraceIdMiddleware;

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
                app.UseMiddleware<ValidarTokenMiddleware>();
                app.UseMiddleware<RecibirTraceIdMiddleware>();
                app.UseMiddleware<ManejoErroresMiddleware>();
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
        public static async Task<WebApplication> ConfigurarServiciosGatewayAsync(this WebApplication app)
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
                //app.UseMiddleware<ValidarTokenMiddleware>();
                app.UseMiddleware<CrearTokenMiddleware>();
                app.UseMiddleware<EnviarTraceIdMiddleware>();
                app.UseMiddleware<EncriptarRespuestaMiddleware>();
                await app.UseOcelot();           
                app.Run();
                return app;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
