using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Usuarios.Entities;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.Services.ObtenerUsuariosRolesService
{
    /// <summary>
    /// Servicio para consultar los roles asignados a un usuario.
    /// </summary>
    public class ObtenerUsuariosRolesService : IObtenerUsuariosRolesService
    {
        private readonly ILogger<ObtenerUsuariosRolesService> _logger;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Inicializa una nueva instancia de ObtenerUsuariosRolesService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        public ObtenerUsuariosRolesService(ILogger<ObtenerUsuariosRolesService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        private const string MENSAJE_ROL_NO_ENCONTRADO = "No se encontró un rol activo para el usuario.";

        /// <summary>
        /// Retorna el rol activo asignado al usuario indicado.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="idUsuario">Identificador del usuario.</param>
        /// <returns>El rol activo del usuario o NotFound si no tiene rol asignado.</returns>
        public async Task<IActionResult> ObtenerRolPorUsuarioAsync(string traceId, int idUsuario)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var usuarioRol = await consultarRepository.ConsultarAsync<UsuarioRolEntity>(traceId, u => u.IdUsuario == idUsuario && u.Activo);
                if (usuarioRol is null) throw new KeyNotFoundException(MENSAJE_ROL_NO_ENCONTRADO);
                var respuesta = new UsuarioRolRespuestaModel();
                respuesta.Id = usuarioRol.Id;
                respuesta.IdUsuario = usuarioRol.IdUsuario;
                respuesta.IdRol = usuarioRol.IdRol;
                respuesta.FechaRegistro = usuarioRol.FechaRegistro;
                respuesta.Activo = usuarioRol.Activo;
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
