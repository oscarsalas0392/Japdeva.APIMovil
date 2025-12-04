using Japdeva.APIMovil.Reclamos.Data;
using Japdeva.APIMovil.Common.Extensions;

WebApplication.CreateBuilder(args)
.AgregarPostgreSQL<ReclamoDbContext>()
.AgregarServiciosMicroservicios()
.Build()
.ConfigurarServiciosMicroservicios();
