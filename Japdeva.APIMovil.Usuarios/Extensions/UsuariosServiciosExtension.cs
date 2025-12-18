using Japdeva.APIMovil.Usuarios.Services.ActualizarUsuarioService;
using Japdeva.APIMovil.Usuarios.Services.CrearUsuarioService;
using Japdeva.APIMovil.Usuarios.Services.EliminarUsuarioService;
using Japdeva.APIMovil.Usuarios.Services.ObtenerUsuariosService;

namespace Japdeva.APIMovil.Usuarios.Extensions
{
    /// <summary>
    /// Extensión para registrar los servicios específicos del proyecto Usuarios.
    /// </summary>
    public static class UsuariosServiciosExtension
    {
        /// <summary>
        /// Agrega los servicios específicos del proyecto Usuarios al contenedor de inyección de dependencias.
        /// </summary>
        /// <param name="builder">El builder de la aplicación web.</param>
        /// <returns>El builder configurado con los servicios de Usuarios.</returns>
        public static WebApplicationBuilder AgregarServiciosUsuarios(this WebApplicationBuilder builder)
        {
            try
            {
                if (builder is null) throw new ArgumentNullException(nameof(builder));

                // Registrar servicios específicos de Usuarios
                builder.Services.AddSingleton<ICrearUsuarioService, CrearUsuarioService>();
                builder.Services.AddSingleton<IActualizarUsuarioService, ActualizarUsuarioService>();
                builder.Services.AddSingleton<IObtenerUsuariosService, ObtenerUsuariosService>();
                builder.Services.AddSingleton<IEliminarUsuarioService, EliminarUsuarioService>();

                return builder;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
