using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Usuarios.Entities;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.Services.ActualizarUsuarioService
{
    /// <summary>
    /// Servicio para la actualización de usuarios en el sistema.
    /// </summary>
    public class ActualizarUsuarioService : IActualizarUsuarioService
    {
        private readonly ILogger<ActualizarUsuarioService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const string MENSAJE_USUARIO_NO_ENCONTRADO = "Usuario no encontrado.";

        /// <summary>
        /// Inicializa una nueva instancia de la clase ActualizarUsuarioService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        public ActualizarUsuarioService(ILogger<ActualizarUsuarioService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Actualiza un usuario existente.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="solicitud">Datos del usuario a actualizar.</param>
        /// <returns>Resultado de la operación de actualización.</returns>
        public async Task<IActionResult> ActualizarUsuarioAsync(string traceId, ActualizarUsuarioSolicitudModel solicitud)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (solicitud is null) throw new ArgumentNullException(nameof(solicitud));
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var actualizarRepository = scope.ServiceProvider.GetRequiredService<IActualizarRepository>();
                Expression<Func<UsuarioEntity, bool>> filtro = u => u.Id == solicitud.Id;
                var usuarioExistente = await consultarRepository.ConsultarAsync<UsuarioEntity>(traceId, filtro);
                if (usuarioExistente is null) throw new KeyNotFoundException(MENSAJE_USUARIO_NO_ENCONTRADO);
                usuarioExistente.Nombre = solicitud.Nombre;
                usuarioExistente.Apellidos = solicitud.Apellidos;
                usuarioExistente.Correo = solicitud.Correo;
                usuarioExistente.Activo = solicitud.Activo;
                usuarioExistente.FechaEdicion = DateTime.UtcNow;
                await actualizarRepository.ActualizarAsync<UsuarioEntity>(traceId, usuarioExistente);
                var respuesta = new UsuarioRespuestaModel();
                respuesta.Id = usuarioExistente.Id;
                respuesta.Identificacion = usuarioExistente.Identificacion;
                respuesta.IdTipoCedula = usuarioExistente.IdTipoCedula;
                respuesta.Nombre = usuarioExistente.Nombre;
                respuesta.Apellidos = usuarioExistente.Apellidos;
                respuesta.Correo = usuarioExistente.Correo;
                respuesta.FechaRegistro = usuarioExistente.FechaRegistro;
                respuesta.FechaEdicion = usuarioExistente.FechaEdicion;
                respuesta.Activo = usuarioExistente.Activo;
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
