using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
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
        private const string MENSAJE_USUARIO_ACTUALIZADO = "Usuario actualizado correctamente.";

        /// <summary>
        /// Inicializa una nueva instancia de la clase ActualizarUsuarioService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para la obtención de dependencias.</param>
        public ActualizarUsuarioService(
            ILogger<ActualizarUsuarioService> logger,
            IServiceProvider serviceProvider)
            => (this._logger, this._serviceProvider) = (logger, serviceProvider);

        /// <summary>
        /// Actualiza un usuario existente.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="solicitud">Datos del usuario a actualizar</param>
        /// <returns>Resultado de la operación de actualización</returns>
        public async Task<IActionResult> ActualizarUsuarioAsync(string traceId, ActualizarUsuarioSolicitudModel solicitud)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            using var scope = this._serviceProvider.CreateScope();
            var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
            var actualizarRepository = scope.ServiceProvider.GetRequiredService<IActualizarRepository>();

            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                if (solicitud is null)
                    throw new ArgumentNullException(nameof(solicitud));

                // Validar que el usuario exista
                Expression<Func<UsuarioEntity, bool>> filtro = u => u.Id == solicitud.Id;
                var usuarioExistente = await consultarRepository.ConsultarAsync<UsuarioEntity>(traceId, filtro);

                if (usuarioExistente is null)
                    return new NotFoundObjectResult(new RespuestaModel
                    {
                        Mensaje = MENSAJE_USUARIO_NO_ENCONTRADO,
                    });

                // Actualizar datos del usuario
                usuarioExistente.Nombre = solicitud.Nombre;
                usuarioExistente.Correo = solicitud.Correo;
                usuarioExistente.Telefono = solicitud.Telefono;
                usuarioExistente.Activo = solicitud.Activo;
                usuarioExistente.FechaActualizacion = DateTime.UtcNow;

                await actualizarRepository.ActualizarAsync<UsuarioEntity>(traceId, usuarioExistente);

                // Construir respuesta
                var respuesta = new UsuarioRespuestaModel
                {
                    Id = usuarioExistente.Id,
                    Nombre = usuarioExistente.Nombre,
                    Correo = usuarioExistente.Correo,
                    Telefono = usuarioExistente.Telefono,
                    FechaCreacion = usuarioExistente.FechaCreacion,
                    FechaActualizacion = usuarioExistente.FechaActualizacion,
                    Activo = usuarioExistente.Activo
                };

                return new OkObjectResult(new RespuestaModel
                {
                    Mensaje = MENSAJE_USUARIO_ACTUALIZADO,
                    Datos = respuesta
                });
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
