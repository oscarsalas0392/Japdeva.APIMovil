using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Reclamos.Data;
using Japdeva.APIMovil.Reclamos.Extensions;

WebApplication.CreateBuilder(args)
.AgregarPostgreSQL<ReclamoDbContext>()
.AgregarServiciosMicroservicios()
.AgregarServiciosReclamos()
.Build()
.ConfigurarServiciosMicroservicios();
