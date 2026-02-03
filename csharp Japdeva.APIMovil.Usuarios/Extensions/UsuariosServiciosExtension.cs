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
        /// Registra los servicios CRUD como Scoped para garantizar una instancia por solicitud HTTP
        /// y permitir la inyección de dependencias scoped (repositorios).
        /// </summary>
        /// <param name="builder">El builder de la aplicación web.</param>
        /// <returns>El builder configurado con los servicios de Usuarios.</returns>
        /// <exception cref="ArgumentNullException">Se lanza cuando el builder es null.</exception>
        public static WebApplicationBuilder AgregarServiciosUsuarios(this WebApplicationBuilder builder)
        {
            try
            {
                // Registrar servicios como Scoped para permitir dependencias scoped (repositorios)
                builder.Services.AddScoped<ICrearUsuarioService, CrearUsuarioService>();
                builder.Services.AddScoped<IActualizarUsuarioService, ActualizarUsuarioService>();
                builder.Services.AddScoped<IObtenerUsuariosService, ObtenerUsuariosService>();
                builder.Services.AddScoped<IEliminarUsuarioService, EliminarUsuarioService>();

                return builder;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al registrar los servicios de Usuarios.", ex);
            }
        }
    }
}