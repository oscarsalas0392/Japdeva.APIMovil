using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Usuarios.Entities;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.Services.CrearUsuarioService
{
    /// <summary>
    /// Servicio para la creación de usuarios en el sistema.
    /// </summary>
    public class CrearUsuarioService : ICrearUsuarioService
    {
        private readonly ILogger<CrearUsuarioService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const string MENSAJE_USUARIO_EXISTE = "El usuario con este correo ya existe.";
        private const string NOMBRE_ACCION_OBTENER = "ObtenerUsuarioPorId";
        private const string NOMBRE_CONTROLADOR = "Usuario";
        private const bool ACTIVO = true;

        /// <summary>
        /// Inicializa una nueva instancia de la clase CrearUsuarioService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para la obtención de dependencias.</param>
        public CrearUsuarioService(
            ILogger<CrearUsuarioService> logger,
            IServiceProvider serviceProvider)
            => (this._logger, this._serviceProvider) = (logger, serviceProvider);

        /// <summary>
        /// Crea un nuevo usuario.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="solicitud">Datos del usuario a crear</param>
        /// <returns>Resultado de la operación de creación</returns>
        public async Task<IActionResult> CrearUsuarioAsync(string traceId, CrearUsuarioSolicitudModel solicitud)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            using var scope = this._serviceProvider.CreateScope();
            var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
            var agregarRepository = scope.ServiceProvider.GetRequiredService<IAgregarRepository>();

            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                if (solicitud is null)
                    throw new ArgumentNullException(nameof(solicitud));

                // Validar que el usuario no exista
                Expression<Func<UsuarioEntity, bool>> filtro = u => u.Correo == solicitud.Correo;
                var usuarioExistente = await consultarRepository.ConsultarAsync<UsuarioEntity>(traceId, filtro);

                if (usuarioExistente is not null)
                    throw new ArgumentException(MENSAJE_USUARIO_EXISTE);
                //VALIDAR QUE NO VENGAN VACIOS, FORMATO VALIDO DE CORREO, CONTRASEÑA FUERTE, 

                // Crear nuevo usuario
                var nuevoUsuario = new UsuarioEntity
                {
                    Identificacion = solicitud.Identificacion,
                    Nombre = solicitud.Nombre,
                    Apellidos = solicitud.Apellidos,
                    Correo = solicitud.Correo,
                    Contrasena = solicitud.Contrasena,
                    Telefono = solicitud.Telefono,
                    FechaCreacion = DateTime.UtcNow,
                    Activo = ACTIVO
                };

                await agregarRepository.AgregarAsync<UsuarioEntity>(traceId, nuevoUsuario);

                // Construir respuesta
                var respuesta = new UsuarioRespuestaModel
                {
                    Id = nuevoUsuario.Id,
                    Nombre = nuevoUsuario.Nombre,                 
                    Correo = nuevoUsuario.Correo,
                    Telefono = nuevoUsuario.Telefono,
                    FechaCreacion = DateTime.Now,
                    FechaActualizacion = DateTime.Now,
                    Activo = ACTIVO
                };

                return new CreatedAtActionResult(NOMBRE_ACCION_OBTENER, NOMBRE_CONTROLADOR, new { id = nuevoUsuario.Id }, respuesta);
            }
            catch (ArgumentException ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw;
            }
            catch (Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(traceId, nombreMetodo);
            }
        }
    }
}
