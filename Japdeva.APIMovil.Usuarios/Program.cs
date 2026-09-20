using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Usuarios.Data;
using Japdeva.APIMovil.Usuarios.Extensions;

WebApplication.CreateBuilder(args)
.AgregarPostgreSQL<UsuariosDBContext>()
.AgregarServiciosMicroservicios()
.AgregarClienteColasGrpc()
.AgregarServiciosUsuarios()
.Build()
.ConfigurarServiciosMicroservicios();