using Japdeva.APIMovil.Colas.Data;
using Japdeva.APIMovil.Common.Extensions;

WebApplication.CreateBuilder(args)
.AgregarPostgreSQL<ColasDbContext>()
.AgregarServiciosMicroservicios()
.Build()
.ConfigurarServiciosMicroservicios();