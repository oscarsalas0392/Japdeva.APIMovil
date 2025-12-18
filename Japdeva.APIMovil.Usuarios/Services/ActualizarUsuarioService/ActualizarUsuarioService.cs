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
        private readonly IActualizarRepository _actualizarRepository;
        private readonly IConsultarRepository _consultarRepository;
        private const string MENSAJE_USUARIO_NO_ENCONTRADO = "Usuario no encontrado.";
        private const string MENSAJE_USUARIO_ACTUALIZADO = "Usuario actualizado correctamente.";
        private const bool EXITO = true;
        private const bool ERROR = false;


        /// <summary>
        /// Inicializa una nueva instancia de la clase ActualizarUsuarioService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="actualizarRepository">Repositorio para actualizar entidades.</param>
        /// <param name="consultarRepository">Repositorio para consultar entidades.</param>
        public ActualizarUsuarioService(ILogger<ActualizarUsuarioService> logger, IActualizarRepository actualizarRepository, IConsultarRepository consultarRepository)
        {
            this._logger = logger;
            this._actualizarRepository = actualizarRepository ?? throw new ArgumentNullException(nameof(actualizarRepository));
            this._consultarRepository = consultarRepository ?? throw new ArgumentNullException(nameof(consultarRepository));
        }

        /// <summary>
        /// Actualiza un usuario existente.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="solicitud">Datos del usuario a actualizar</param>
        /// <returns>Resultado de la operación de actualización</returns>
        public async Task<IActionResult> ActualizarUsuarioAsync(string traceId, ActualizarUsuarioSolicitudModel solicitud)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                if (solicitud is null) throw new ArgumentNullException(nameof(solicitud));

                Expression<Func<UsuarioEntity, bool>> filtro = u => u.Id == solicitud.Id;
                var usuarioExistente = await this._consultarRepository.ConsultarAsync<UsuarioEntity>(traceId, filtro);
                if (usuarioExistente is null)
                    return new NotFoundObjectResult(new RespuestaModel 
                    { 
                        Mensaje = MENSAJE_USUARIO_NO_ENCONTRADO, 
                        Exito = ERROR 
                    });

                usuarioExistente.Nombre = solicitud.Nombre;
                usuarioExistente.Correo = solicitud.Correo;
                usuarioExistente.Telefono = solicitud.Telefono;
                usuarioExistente.Activo = solicitud.Activo;
                usuarioExistente.FechaActualizacion = DateTime.UtcNow;

                await this._actualizarRepository.ActualizarAsync<UsuarioEntity>(traceId, usuarioExistente);

                var respuesta = new UsuarioRespuestaModel();
                respuesta.Id = usuarioExistente.Id;
                respuesta.Nombre = usuarioExistente.Nombre;
                respuesta.Correo = usuarioExistente.Correo;
                respuesta.Telefono = usuarioExistente.Telefono;
                respuesta.FechaCreacion = usuarioExistente.FechaCreacion;
                respuesta.FechaActualizacion = usuarioExistente.FechaActualizacion;
                respuesta.Activo = usuarioExistente.Activo;

                return new OkObjectResult(new RespuestaModel 
                { 
                    Mensaje = MENSAJE_USUARIO_ACTUALIZADO, 
                    Exito = EXITO, 
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
