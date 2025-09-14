using Japdeva.APIMovil.Common.Extensions;

WebApplication.CreateBuilder(args)
.AgregarServiciosMicroservicios()
.Build()
.ConfigurarServiciosMicroservicios();