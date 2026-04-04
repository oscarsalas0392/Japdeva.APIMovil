using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Usuarios.Entities;

namespace Japdeva.APIMovil.Usuarios.Services.EliminarUsuarioService
{
    /// <summary>
    /// Servicio para la eliminación de usuarios en el sistema.
    /// </summary>
    public class EliminarUsuarioService : IEliminarUsuarioService
    {
        private readonly ILogger<EliminarUsuarioService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const string MENSAJE_USUARIO_NO_ENCONTRADO = "Usuario no encontrado.";
        private const string MENSAJE_USUARIO_ELIMINADO = "Usuario eliminado correctamente.";
        private const bool EXITO = true;
        private const bool ACTIVO = false;

        /// <summary>
        /// Inicializa una nueva instancia de la clase EliminarUsuarioService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        public EliminarUsuarioService(ILogger<EliminarUsuarioService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Elimina un usuario por su identificador.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Identificador del usuario a eliminar.</param>
        /// <returns>Resultado de la operación de eliminación.</returns>
        public async Task<IActionResult> EliminarUsuarioAsync(string traceId, int id)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var actualizarRepository = scope.ServiceProvider.GetRequiredService<IActualizarRepository>();
                var usuarioExistente = await consultarRepository.ConsultarAsync<UsuarioEntity>(traceId, u => u.Id == id);
                if (usuarioExistente is null) throw new KeyNotFoundException(MENSAJE_USUARIO_NO_ENCONTRADO);
                usuarioExistente.Activo = ACTIVO;
                await actualizarRepository.ActualizarAsync<UsuarioEntity>(traceId, usuarioExistente);
                return new OkObjectResult(new RespuestaModel { Mensaje = MENSAJE_USUARIO_ELIMINADO, Exito = EXITO });
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
