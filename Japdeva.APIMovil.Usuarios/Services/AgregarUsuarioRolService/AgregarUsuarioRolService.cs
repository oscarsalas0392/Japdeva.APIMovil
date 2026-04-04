using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Japdeva.APIMovil.Common.Extensions;
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
        private readonly IServiceProvider _serviceProvider;
        private const string MENSAJE_ASIGNACION_EXISTE = "El usuario ya tiene asignado ese rol.";
        private const bool EXITO = true;

        /// <summary>
        /// Inicializa una nueva instancia de AgregarUsuarioRolService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        public AgregarUsuarioRolService(ILogger<AgregarUsuarioRolService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
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
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var agregarRepository = scope.ServiceProvider.GetRequiredService<IAgregarRepository>();
                Expression<Func<UsuarioRolEntity, bool>> filtro = ur => ur.IdUsuario == solicitud.IdUsuario && ur.IdRol == solicitud.IdRol && ur.Activo;
                var asignacionExistente = await consultarRepository.ConsultarAsync<UsuarioRolEntity>(traceId, filtro);
                if (asignacionExistente is not null) throw new ArgumentException(MENSAJE_ASIGNACION_EXISTE);

                var nuevaAsignacion = new UsuarioRolEntity();
                nuevaAsignacion.IdUsuario = solicitud.IdUsuario;
                nuevaAsignacion.IdRol = solicitud.IdRol;
                nuevaAsignacion.IdUsuarioAdministrador = solicitud.IdUsuarioAdministrador;
                nuevaAsignacion.FechaRegistro = DateTime.UtcNow;
                nuevaAsignacion.Activo = EXITO;
                await agregarRepository.AgregarAsync<UsuarioRolEntity>(traceId, nuevaAsignacion);
                var respuesta = new UsuarioRolRespuestaModel();
                respuesta.Id = nuevaAsignacion.Id;
                respuesta.IdUsuario = nuevaAsignacion.IdUsuario;
                respuesta.IdRol = nuevaAsignacion.IdRol;
                respuesta.IdUsuarioAdministrador = nuevaAsignacion.IdUsuarioAdministrador;
                respuesta.FechaRegistro = nuevaAsignacion.FechaRegistro;
                respuesta.Activo = nuevaAsignacion.Activo;
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
