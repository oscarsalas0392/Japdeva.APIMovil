using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Ocelot.DependencyInjection;

namespace Japdeva.APIMovil.Common.Extensions
{
    public static class AgregarServiciosExtension
    {
        public static void AgregarServiciosMicroservicios(this WebApplicationBuilder builder) 
        {
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
        }

        public static void AgregarServiciosGateway(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddOcelot(builder.Configuration);
        }
    }
}
