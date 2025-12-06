using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Reclamos.Data;

WebApplication.CreateBuilder(args)
.AgregarPostgreSQL<ReclamoDbContext>()
.AgregarServiciosMicroservicios()
.Build()
.ConfigurarServiciosMicroservicios();
