using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Usuarios.Data;

WebApplication.CreateBuilder(args)
.AgregarPostgreSQL<AdministracionDbContext>()
.AgregarServiciosMicroservicios()
.Build()
.ConfigurarServiciosMicroservicios();