using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.EnvioCorreos.Data;
using Japdeva.APIMovil.EnvioCorreos.Extensions;

WebApplication.CreateBuilder(args)
.AgregarPostgreSQL<EnvioCorreosDBContext>()
.AgregarServiciosMicroservicios()
.AgregarServiciosEnvioCorreos()
.Build()
.ConfigurarServiciosMicroservicios();
