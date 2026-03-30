using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Usuarios.Entities;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.Services.AgregarUsuarioRolService
{
    /// <summary>
    /// Servicio para la asignación de roles a usuarios.
    /// </summary>
    public class AgregarUsuarioRolService : IAgregarUsuarioRolService
    {
        private readonly ILogger<AgregarUsuarioRolService> _logger;
        private readonly IAgregarRepository _agregarRepository;
        private readonly IConsultarRepository _consultarRepository;
        private const string MENSAJE_ROL_ASIGNADO = "Rol asignado al usuario correctamente.";
        private const string MENSAJE_ASIGNACION_EXISTE = "El usuario ya tiene asignado ese rol.";
        private const bool EXITO = true;
        private const bool ERROR = false;

        /// <summary>
        /// Inicializa una nueva instancia de AgregarUsuarioRolService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="agregarRepository">Repositorio para agregar entidades.</param>
        /// <param name="consultarRepository">Repositorio para consultar entidades.</param>
        public AgregarUsuarioRolService(ILogger<AgregarUsuarioRolService> logger, IAgregarRepository agregarRepository, IConsultarRepository consultarRepository)
        {
            this._logger = logger;
            this._agregarRepository = agregarRepository ?? throw new ArgumentNullException(nameof(agregarRepository));
            this._consultarRepository = consultarRepository ?? throw new ArgumentNullException(nameof(consultarRepository));
        }

        /// <summary>
        /// Asigna un rol a un usuario.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="solicitud">Datos de la asignación de rol.</param>
        /// <returns>Resultado de la operación de asignación.</returns>
        public async Task<IActionResult> AgregarUsuarioRolAsync(string traceId, AgregarUsuarioRolSolicitudModel solicitud)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                if (solicitud is null) throw new ArgumentNullException(nameof(solicitud));

                Expression<Func<UsuarioRolEntity, bool>> filtro = ur => ur.IdUsuario == solicitud.IdUsuario && ur.IdRol == solicitud.IdRol && ur.Activo;
                var asignacionExistente = await this._consultarRepository.ConsultarAsync<UsuarioRolEntity>(traceId, filtro);
                if (asignacionExistente is not null)
                    return new BadRequestObjectResult(new RespuestaModel { Mensaje = MENSAJE_ASIGNACION_EXISTE, Exito = ERROR });

                var nuevaAsignacion = new UsuarioRolEntity();
                nuevaAsignacion.IdUsuario = solicitud.IdUsuario;
                nuevaAsignacion.IdRol = solicitud.IdRol;
                nuevaAsignacion.IdUsuarioAdministrador = solicitud.IdUsuarioAdministrador;
                nuevaAsignacion.FechaRegistro = DateTime.UtcNow;
                nuevaAsignacion.Activo = EXITO;

                await this._agregarRepository.AgregarAsync<UsuarioRolEntity>(traceId, nuevaAsignacion);

                var respuesta = new UsuarioRolRespuestaModel();
                respuesta.Id = nuevaAsignacion.Id;
                respuesta.IdUsuario = nuevaAsignacion.IdUsuario;
                respuesta.IdRol = nuevaAsignacion.IdRol;
                respuesta.IdUsuarioAdministrador = nuevaAsignacion.IdUsuarioAdministrador;
                respuesta.FechaRegistro = nuevaAsignacion.FechaRegistro;
                respuesta.Activo = nuevaAsignacion.Activo;

                return new OkObjectResult(new RespuestaModel { Mensaje = MENSAJE_ROL_ASIGNADO, Exito = EXITO, Datos = respuesta });
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
