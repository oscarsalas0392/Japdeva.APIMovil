
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Ocelot.Middleware;
using Japdeva.APIMovil.Common.Middlewares;

using Japdeva.APIMovil.Common.Middlewares.EnviarTraceIdMiddleware;

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
                app.UseMiddleware<ManejoErroresMiddleware>();
                app.UseAuthentication();
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
        /// <returns>Una tarea que representa la operación asíncrona. El valor de la tarea contiene la aplicación web configurada.</returns>
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
                app.UseMiddleware<ManejoErroresMiddleware>();
                app.UseAuthorization();
                app.MapControllers();
                app.UseMiddleware<LimitarSolicitudesMiddleware>();
                app.UseMiddleware<ValidarTokenMiddleware>();
                app.UseMiddleware<CrearTokenMiddleware>();
                app.UseMiddleware<EnviarTraceIdMiddleware>();
                app.UseMiddleware<GestionarTokenRespuestaMiddleware>();
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
