using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Usuarios.Entities;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.Services.AgregarDepartamentoUsuarioService
{
    /// <summary>
    /// Servicio para la asignación de usuarios a departamentos.
    /// </summary>
    public class AgregarDepartamentoUsuarioService : IAgregarDepartamentoUsuarioService
    {
        private readonly ILogger<AgregarDepartamentoUsuarioService> _logger;
        private readonly IAgregarRepository _agregarRepository;
        private readonly IConsultarRepository _consultarRepository;
        private const string MENSAJE_ASIGNADO = "Usuario asignado al departamento correctamente.";
        private const string MENSAJE_ASIGNACION_EXISTE = "El usuario ya está asignado a ese departamento.";
        private const bool EXITO = true;
        private const bool ERROR = false;

        /// <summary>
        /// Inicializa una nueva instancia de AgregarDepartamentoUsuarioService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="agregarRepository">Repositorio para agregar entidades.</param>
        /// <param name="consultarRepository">Repositorio para consultar entidades.</param>
        public AgregarDepartamentoUsuarioService(ILogger<AgregarDepartamentoUsuarioService> logger, IAgregarRepository agregarRepository, IConsultarRepository consultarRepository)
        {
            this._logger = logger;
            this._agregarRepository = agregarRepository ?? throw new ArgumentNullException(nameof(agregarRepository));
            this._consultarRepository = consultarRepository ?? throw new ArgumentNullException(nameof(consultarRepository));
        }

        /// <summary>
        /// Asigna un usuario a un departamento.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="solicitud">Datos de la asignación.</param>
        /// <returns>Resultado de la operación de asignación.</returns>
        public async Task<IActionResult> AgregarDepartamentoUsuarioAsync(string traceId, AgregarDepartamentoUsuarioSolicitudModel solicitud)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                if (solicitud is null) throw new ArgumentNullException(nameof(solicitud));

                Expression<Func<DepartamentoUsuarioEntity, bool>> filtro =
                    du => du.IdUsuario == solicitud.IdUsuario && du.IdDepartamento == solicitud.IdDepartamento && du.Activo;
                var asignacionExistente = await this._consultarRepository.ConsultarAsync<DepartamentoUsuarioEntity>(traceId, filtro);
                if (asignacionExistente is not null)
                    return new BadRequestObjectResult(new RespuestaModel { Mensaje = MENSAJE_ASIGNACION_EXISTE, Exito = ERROR });

                var nuevaAsignacion = new DepartamentoUsuarioEntity();
                nuevaAsignacion.IdUsuario = solicitud.IdUsuario;
                nuevaAsignacion.IdDepartamento = solicitud.IdDepartamento;
                nuevaAsignacion.IdUsuarioAdministrador = solicitud.IdUsuarioAdministrador;
                nuevaAsignacion.FechaRegistro = DateTime.UtcNow;
                nuevaAsignacion.Activo = EXITO;

                await this._agregarRepository.AgregarAsync<DepartamentoUsuarioEntity>(traceId, nuevaAsignacion);

                var respuesta = new DepartamentoUsuarioRespuestaModel();
                respuesta.Id = nuevaAsignacion.Id;
                respuesta.IdUsuario = nuevaAsignacion.IdUsuario;
                respuesta.IdDepartamento = nuevaAsignacion.IdDepartamento;
                respuesta.IdUsuarioAdministrador = nuevaAsignacion.IdUsuarioAdministrador;
                respuesta.FechaRegistro = nuevaAsignacion.FechaRegistro;
                respuesta.Activo = nuevaAsignacion.Activo;

                return new OkObjectResult(new RespuestaModel { Mensaje = MENSAJE_ASIGNADO, Exito = EXITO, Datos = respuesta });
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
