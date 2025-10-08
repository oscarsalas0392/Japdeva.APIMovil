using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Usuarios.Data;

WebApplication.CreateBuilder(args)
.AgregarServiciosMicroservicios()
.AgregarPostgreSQL<AdministracionDbContext>()
.Build()
.ConfigurarServiciosMicroservicios();