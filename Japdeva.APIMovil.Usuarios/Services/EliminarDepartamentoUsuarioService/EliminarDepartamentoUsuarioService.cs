using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Usuarios.Entities;


namespace Japdeva.APIMovil.Usuarios.Services.EliminarDepartamentoUsuarioService
{
    /// <summary>
    /// Servicio para la eliminación de asignaciones de usuarios a departamentos.
    /// </summary>
    public class EliminarDepartamentoUsuarioService : IEliminarDepartamentoUsuarioService
    {
        private readonly ILogger<EliminarDepartamentoUsuarioService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const string MENSAJE_NO_ENCONTRADO = "Asignación de departamento no encontrada.";
        private const bool ACTIVO = false;

        /// <summary>
        /// Inicializa una nueva instancia de EliminarDepartamentoUsuarioService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        public EliminarDepartamentoUsuarioService(ILogger<EliminarDepartamentoUsuarioService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Elimina la asignación de un usuario a un departamento por su identificador.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Identificador de la asignación a eliminar.</param>
        /// <returns>Resultado de la operación de eliminación.</returns>
        public async Task<IActionResult> EliminarDepartamentoUsuarioAsync(string traceId, long id)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var actualizarRepository = scope.ServiceProvider.GetRequiredService<IActualizarRepository>();
                var asignacion = await consultarRepository.ConsultarAsync<DepartamentoUsuarioEntity>(traceId, asociacion => asociacion.Id == id);
                if (asignacion is null) throw new KeyNotFoundException(MENSAJE_NO_ENCONTRADO);
                asignacion.Activo = ACTIVO;
                await actualizarRepository.ActualizarAsync<DepartamentoUsuarioEntity>(traceId, asignacion);
                return new OkResult();
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
