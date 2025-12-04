using Japdeva.APIMovil.Colas.Data;
using Japdeva.APIMovil.Colas.Extensions;
using Japdeva.APIMovil.Common.Extensions;

WebApplication.CreateBuilder(args)
.AgregarPostgreSQL<ColasDbContext>()
.AgregarServiciosMicroservicios()
.AgregarServiciosColas()
.Build()
.ConfigurarServiciosMicroservicios();