using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Japdeva.APIMovil.Common.Extensions;
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
        private readonly IServiceProvider _serviceProvider;
        private const string MENSAJE_ASIGNACION_EXISTE = "El usuario ya está asignado a ese departamento.";
        private const string MENSAJE_USUARIO_NO_EXISTE = "El usuario no existe.";
        private const string MENSAJE_DEPARTAMENTO_NO_EXISTE = "El departamento no existe.";
        private const bool EXITO = true;


        /// <summary>
        /// Inicializa una nueva instancia de AgregarDepartamentoUsuarioService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        public AgregarDepartamentoUsuarioService(ILogger<AgregarDepartamentoUsuarioService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
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
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var agregarRepository = scope.ServiceProvider.GetRequiredService<IAgregarRepository>();

                var usuario = await consultarRepository.ConsultarAsync<UsuarioEntity>(traceId, usuario => usuario.Id == solicitud.IdUsuario &&  usuario.Activo);
                if (usuario is null) throw new ArgumentException(MENSAJE_USUARIO_NO_EXISTE);

                var departamento = await consultarRepository.ConsultarAsync<DepartamentoEntity>(traceId, departamento => departamento.Id == solicitud.IdDepartamento && usuario.Activo);
                if (departamento is null) throw new ArgumentException(MENSAJE_DEPARTAMENTO_NO_EXISTE);

                var asignacionExistente = await consultarRepository.ConsultarAsync<DepartamentoUsuarioEntity>(traceId, 
                    asociacion => asociacion.IdUsuario == solicitud.IdUsuario && asociacion.IdDepartamento == solicitud.IdDepartamento && asociacion.Activo);

                if (asignacionExistente is not null) throw new ArgumentException(MENSAJE_ASIGNACION_EXISTE);

                var nuevaAsignacion = new DepartamentoUsuarioEntity();
                nuevaAsignacion.IdUsuario = solicitud.IdUsuario;
                nuevaAsignacion.IdDepartamento = solicitud.IdDepartamento;
                nuevaAsignacion.IdUsuarioAdministrador = solicitud.IdUsuarioAdministrador;
                nuevaAsignacion.FechaRegistro = DateTime.UtcNow;
                nuevaAsignacion.Activo = EXITO;

                await agregarRepository.AgregarAsync<DepartamentoUsuarioEntity>(traceId, nuevaAsignacion);

                var respuesta = new DepartamentoUsuarioRespuestaModel();
                respuesta.Id = nuevaAsignacion.Id;
                respuesta.IdUsuario = nuevaAsignacion.IdUsuario;
                respuesta.IdDepartamento = nuevaAsignacion.IdDepartamento;
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
