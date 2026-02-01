using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Parametros.Data;
using Japdeva.APIMovil.Parametros.Extensions;

WebApplication.CreateBuilder(args)
.AgregarPostgreSQL<ParametroDbContext>()
.AgregarServiciosMicroservicios()
.AgregarServiciosParametros()
.Build()
.ConfigurarServiciosMicroservicios();