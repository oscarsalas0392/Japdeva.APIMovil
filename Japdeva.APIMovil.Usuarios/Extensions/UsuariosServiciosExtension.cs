using Japdeva.APIMovil.Usuarios.BackgroundServices;
using Japdeva.APIMovil.Usuarios.Services.ActualizarContrasenaUsuarioService;
using Japdeva.APIMovil.Usuarios.Services.ActualizarUsuarioRolService;
using Japdeva.APIMovil.Usuarios.Services.ActualizarUsuarioService;
using Japdeva.APIMovil.Usuarios.Services.AgregarDepartamentoUsuarioService;
using Japdeva.APIMovil.Usuarios.Services.AgregarUsuarioRolService;
using Japdeva.APIMovil.Usuarios.Services.AgregarUsuarioService;
using Japdeva.APIMovil.Usuarios.Services.AutenticarUsuarioService;
using Japdeva.APIMovil.Usuarios.Services.CrearUsuarioService;
using Japdeva.APIMovil.Usuarios.Services.DepartamentoCacheService;
using Japdeva.APIMovil.Usuarios.Services.EliminarDepartamentoUsuarioService;
using Japdeva.APIMovil.Usuarios.Services.EliminarUsuarioRolService;
using Japdeva.APIMovil.Usuarios.Services.EliminarUsuarioService;
using Japdeva.APIMovil.Usuarios.Services.NotificarContrasenaTemporalService;
using Japdeva.APIMovil.Usuarios.Services.ObtenerDepartamentosService;
using Japdeva.APIMovil.Usuarios.Services.ObtenerDepartamentosUsuariosService;
using Japdeva.APIMovil.Usuarios.Services.ObtenerRolesService;
using Japdeva.APIMovil.Usuarios.Services.ObtenerTiposCedulaService;
using Japdeva.APIMovil.Usuarios.Services.ObtenerUsuariosRolesService;
using Japdeva.APIMovil.Usuarios.Services.ObtenerUsuariosService;
using Japdeva.APIMovil.Usuarios.Services.OlvidarContrasenaService;
using Japdeva.APIMovil.Usuarios.Services.RolCacheService;
using Japdeva.APIMovil.Usuarios.Services.TipoCedulaCacheService;

namespace Japdeva.APIMovil.Usuarios.Extensions
{
    /// <summary>
    /// Extensión para registrar los servicios del proyecto Usuarios.
    /// </summary>
    public static class UsuariosServiciosExtension
    {
        /// <summary>
        /// Agrega los servicios del proyecto Usuarios al contenedor de inyección de dependencias.
        /// </summary>
        /// <param name="builder">El builder de la aplicación web.</param>
        /// <returns>El builder configurado con los servicios de Usuarios.</returns>
        public static WebApplicationBuilder AgregarServiciosUsuarios(this WebApplicationBuilder builder)
        {
            try
            {
                if (builder is null) throw new ArgumentNullException(nameof(builder));

                builder.Services.AddSingleton<IAutenticarUsuarioService, AutenticarUsuarioService>();
                builder.Services.AddSingleton<IActualizarContrasenaUsuarioService, ActualizarContrasenaUsuarioService>();
                builder.Services.AddSingleton<INotificarContrasenaTemporalService, NotificarContrasenaTemporalService>();
                builder.Services.AddSingleton<IOlvidarContrasenaService, OlvidarContrasenaService>();
                builder.Services.AddSingleton<ICrearUsuarioService, CrearUsuarioService>();
                builder.Services.AddSingleton<IAgregarUsuarioService, AgregarUsuarioService>();
                builder.Services.AddSingleton<IActualizarUsuarioService, ActualizarUsuarioService>();
                builder.Services.AddSingleton<IObtenerUsuariosService, ObtenerUsuariosService>();
                builder.Services.AddSingleton<IEliminarUsuarioService, EliminarUsuarioService>();

                builder.Services.AddSingleton<IRolCacheService, RolCacheService>();
                builder.Services.AddSingleton<ITipoCedulaCacheService, TipoCedulaCacheService>();
                builder.Services.AddSingleton<IDepartamentoCacheService, DepartamentoCacheService>();

                builder.Services.AddSingleton<IObtenerRolesService, ObtenerRolesService>();
                builder.Services.AddSingleton<IObtenerTiposCedulaService, ObtenerTiposCedulaService>();
                builder.Services.AddSingleton<IObtenerDepartamentosService, ObtenerDepartamentosService>();

                builder.Services.AddSingleton<IAgregarUsuarioRolService, AgregarUsuarioRolService>();
                builder.Services.AddSingleton<IActualizarUsuarioRolService, ActualizarUsuarioRolService>();
                builder.Services.AddSingleton<IEliminarUsuarioRolService, EliminarUsuarioRolService>();
                builder.Services.AddSingleton<IObtenerUsuariosRolesService, ObtenerUsuariosRolesService>();

                builder.Services.AddSingleton<IAgregarDepartamentoUsuarioService, AgregarDepartamentoUsuarioService>();
                builder.Services.AddSingleton<IEliminarDepartamentoUsuarioService, EliminarDepartamentoUsuarioService>();
                builder.Services.AddSingleton<IObtenerDepartamentosUsuariosService, ObtenerDepartamentosUsuariosService>();

                builder.Services.AddHostedService<UsuariosBackGroundService>();
                builder.Services.AddHostedService<ObtenerUsuarioBackGroundService>();
                builder.Services.AddHostedService<ObtenerCorreosPorDepartamentoBackGroundService>();
                builder.Services.AddHostedService<ObtenerDepartamentoBackGroundService>();

                return builder;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
