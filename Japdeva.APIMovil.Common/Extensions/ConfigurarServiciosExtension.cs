

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Ocelot.Middleware;

namespace Japdeva.APIMovil.Common.Extensions
{
    public static class ConfigurarServiciosExtension
    {
        public static void ConfigurarServiciosMicroservicios(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }

        public static async Task ConfigurarServiciosGateway(this WebApplication app)
        {
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
    }
}
