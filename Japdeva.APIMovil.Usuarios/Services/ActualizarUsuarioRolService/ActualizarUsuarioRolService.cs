using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Usuarios.Entities;
using Japdeva.APIMovil.Usuarios.Models;
using Japdeva.APIMovil.Usuarios.Services.RolCacheService;

namespace Japdeva.APIMovil.Usuarios.Services.ActualizarUsuarioRolService
{
    /// <summary>
    /// Servicio para la actualización de asignaciones de roles a usuarios.
    /// </summary>
    public class ActualizarUsuarioRolService : IActualizarUsuarioRolService
    {
        private readonly ILogger<ActualizarUsuarioRolService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRolCacheService _rolCacheService;
        private const string MENSAJE_NO_ENCONTRADO = "Asignación de rol no encontrada.";
        private const string MENSAJE_ROL_NO_EXISTE = "El id del rol no existe.";
        private const string MENSAJE_USUARIO_NO_EXISTE = "El id del usuario no existe.";
        private const string MENSAJE_USUARIO_ADMINISTRADOR_NO_EXISTE = "El id del usuario administrador no existe.";
   

        /// <summary>
        /// Inicializa una nueva instancia de ActualizarUsuarioRolService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        /// <param name="rolCacheService">Servicio de caché de roles.</param>
        public ActualizarUsuarioRolService(ILogger<ActualizarUsuarioRolService> logger, IServiceProvider serviceProvider, IRolCacheService rolCacheService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this._rolCacheService = rolCacheService;
        }

        /// <summary>
        /// Actualiza la asignación de un rol a un usuario.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="solicitud">Datos de la actualización.</param>
        /// <returns>Resultado de la operación de actualización.</returns>
        public async Task<IActionResult> ActualizarUsuarioRolAsync(string traceId, ActualizarUsuarioRolSolicitudModel solicitud)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (solicitud is null) throw new ArgumentNullException(nameof(solicitud));

                var rol = this._rolCacheService.ObtenerPorId(traceId, solicitud.IdRol);
                if (rol is null) throw new ArgumentException(MENSAJE_ROL_NO_EXISTE);

                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var actualizarRepository = scope.ServiceProvider.GetRequiredService<IActualizarRepository>();

                UsuarioEntity? usuario = await consultarRepository.ConsultarAsync<UsuarioEntity>(traceId, usuario => usuario.Id == solicitud.IdUsuario && usuario.Activo);
                UsuarioRolEntity? usuarioRol = await consultarRepository.ConsultarAsync<UsuarioRolEntity>(traceId, usuarioRol => usuarioRol.Id == solicitud.Id);

                if (usuario is null) throw new KeyNotFoundException(MENSAJE_USUARIO_NO_EXISTE);
                if (usuarioRol is null) throw new KeyNotFoundException(MENSAJE_NO_ENCONTRADO);

                if (solicitud.IdUsuarioAdministrador.HasValue)
                {
                    var usuarioAdmin = await consultarRepository.ConsultarAsync<UsuarioEntity>(traceId, usuario => usuario.Id == solicitud.IdUsuarioAdministrador && usuario.Activo);
                    if (usuarioAdmin is null) throw new KeyNotFoundException(MENSAJE_USUARIO_ADMINISTRADOR_NO_EXISTE);
                }

                usuarioRol.IdRol = solicitud.IdRol;
                usuarioRol.IdUsuario = solicitud.IdUsuario;
                usuarioRol.IdUsuarioAdministrador = solicitud.IdUsuarioAdministrador;
                usuarioRol.FechaEdicion = DateTime.UtcNow;

                await actualizarRepository.ActualizarAsync<UsuarioRolEntity>(traceId, usuarioRol);
                var respuesta = new UsuarioRolRespuestaModel();
                respuesta.Id = usuarioRol.Id;
                respuesta.IdUsuario = usuarioRol.IdUsuario;
                respuesta.IdRol = usuarioRol.IdRol;
                respuesta.FechaRegistro = usuarioRol.FechaRegistro;
                respuesta.Activo = usuarioRol.Activo;
                return new OkObjectResult(respuesta);
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
