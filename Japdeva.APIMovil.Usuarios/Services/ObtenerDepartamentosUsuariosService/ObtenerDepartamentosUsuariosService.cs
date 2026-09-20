using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Usuarios.Entities;
using Japdeva.APIMovil.Usuarios.Models;
using Japdeva.APIMovil.Usuarios.Services.DepartamentoCacheService;

namespace Japdeva.APIMovil.Usuarios.Services.ObtenerDepartamentosUsuariosService
{
    /// <summary>
    /// Servicio para consultar el departamento asignado a un usuario.
    /// </summary>
    public class ObtenerDepartamentosUsuariosService : IObtenerDepartamentosUsuariosService
    {
        private readonly ILogger<ObtenerDepartamentosUsuariosService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IDepartamentoCacheService _departamentoCacheService;

        private const string MENSAJE_DEPARTAMENTO_NO_ENCONTRADO = "No se encontró un departamento activo para el usuario.";

        /// <summary>
        /// Inicializa una nueva instancia de ObtenerDepartamentosUsuariosService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        /// <param name="departamentoCacheService">Servicio de caché para obtener la descripción del departamento.</param>
        public ObtenerDepartamentosUsuariosService(
            ILogger<ObtenerDepartamentosUsuariosService> logger,
            IServiceProvider serviceProvider,
            IDepartamentoCacheService departamentoCacheService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this._departamentoCacheService = departamentoCacheService;
        }

        /// <summary>
        /// Retorna el departamento activo asignado al usuario indicado, incluyendo su descripción.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="idUsuario">Identificador del usuario.</param>
        /// <returns>El departamento activo del usuario con su descripción, o NotFound si no tiene asignación.</returns>
        public async Task<IActionResult> ObtenerDepartamentoPorUsuarioAsync(string traceId, int idUsuario)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var asignacion = await consultarRepository.ConsultarAsync<DepartamentoUsuarioEntity>(traceId, a => a.IdUsuario == idUsuario && a.Activo);
                if (asignacion is null) throw new KeyNotFoundException(MENSAJE_DEPARTAMENTO_NO_ENCONTRADO);

                var departamento = this._departamentoCacheService.ObtenerPorId(traceId, asignacion.IdDepartamento);

                var respuesta = new DepartamentoUsuarioRespuestaModel();
                respuesta.Id = asignacion.Id;
                respuesta.IdUsuario = asignacion.IdUsuario;
                respuesta.IdDepartamento = asignacion.IdDepartamento;
                respuesta.DescripcionDepartamento = departamento is null ? string.Empty : departamento.Descripcion;
                respuesta.IdUsuarioAdministrador = asignacion.IdUsuarioAdministrador;
                respuesta.FechaRegistro = asignacion.FechaRegistro;
                respuesta.Activo = asignacion.Activo;

                return new OkObjectResult(respuesta);
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
