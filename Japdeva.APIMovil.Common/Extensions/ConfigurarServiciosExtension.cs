
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Ocelot.Middleware;
using Japdeva.APIMovil.Common.Middlewares;
using Japdeva.APIMovil.Common.Middlewares.EnviarTraceIdMiddleware;
using Japdeva.APIMovil.Common.Middlewares.ManejoErroresGatewayMiddleware;

namespace Japdeva.APIMovil.Common.Extensions
{
    /// <summary>
    /// Clase estática que proporciona métodos de extensión para configurar los servicios en la API de Japdeva.
    /// </summary>
    public static class ConfigurarServiciosExtension
    {
        private const string POLITICA_CORS_GATEWAY = "PoliticaCorsGateway";
        private const string USAR_MANEJO_ERRORES_ENV = "MANEJO_ERRORES";
        private const string MANEJO_ERRORES_DEFAULT = "1";

        /// <summary>
        /// Configura los servicios para los microservicios de la aplicación.
        /// </summary>
        /// <param name="app">Instancia de la aplicación web.</param>
        /// 
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

                string usarManejoErrores = Environment.GetEnvironmentVariable(USAR_MANEJO_ERRORES_ENV) ?? MANEJO_ERRORES_DEFAULT;

                if(usarManejoErrores == MANEJO_ERRORES_DEFAULT) app.UseMiddleware<ManejoErroresMiddleware>();
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
                app.UseCors(POLITICA_CORS_GATEWAY);
                app.UseMiddleware<ManejoErroresGatewayMiddleware>();
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
