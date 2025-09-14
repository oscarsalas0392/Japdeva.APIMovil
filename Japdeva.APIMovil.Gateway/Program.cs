using Japdeva.APIMovil.Common.Extensions;

 WebApplication.CreateBuilder(args)
.AgregarServiciosGateway()
.Build()
.ConfigurarServiciosGatewayAsync()
.GetAwaiter()
.GetResult();

