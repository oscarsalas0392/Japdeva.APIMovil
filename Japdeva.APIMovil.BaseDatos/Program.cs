using Japdeva.APIMovil.BaseDatos.Extensions;
using Japdeva.APIMovil.BaseDatos.Services.EjecutarScriptsService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.AgregarServiciosBaseDatos();

IHost host = builder.Build();
IEjecutarScriptsService service = host.Services.GetRequiredService<IEjecutarScriptsService>();
return await service.EjecutarAsync(args);
