using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Common.Repositories.EliminarRepository;
using Japdeva.APIMovil.Usuarios.Entities;

namespace Japdeva.APIMovil.Usuarios.Services.EliminarUsuarioRolService
{
    /// <summary>
    /// Servicio para la eliminación de asignaciones de roles a usuarios.
    /// </summary>
    public class EliminarUsuarioRolService : IEliminarUsuarioRolService
    {
        private readonly ILogger<EliminarUsuarioRolService> _logger;
        private readonly IConsultarRepository _consultarRepository;
        private readonly IEliminarRepository _eliminarRepository;
        private const string MENSAJE_NO_ENCONTRADO = "Asignación de rol no encontrada.";
        private const string MENSAJE_ELIMINADO = "Asignación de rol eliminada correctamente.";
        private const bool EXITO = true;
        private const bool ERROR = false;

        /// <summary>
        /// Inicializa una nueva instancia de EliminarUsuarioRolService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="consultarRepository">Repositorio para consultar entidades.</param>
        /// <param name="eliminarRepository">Repositorio para eliminar entidades.</param>
        public EliminarUsuarioRolService(ILogger<EliminarUsuarioRolService> logger, IConsultarRepository consultarRepository, IEliminarRepository eliminarRepository)
        {
            this._logger = logger;
            this._consultarRepository = consultarRepository ?? throw new ArgumentNullException(nameof(consultarRepository));
            this._eliminarRepository = eliminarRepository ?? throw new ArgumentNullException(nameof(eliminarRepository));
        }

        /// <summary>
        /// Elimina la asignación de un rol a un usuario por su identificador.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Identificador de la asignación a eliminar.</param>
        /// <returns>Resultado de la operación de eliminación.</returns>
        public async Task<IActionResult> EliminarUsuarioRolAsync(string traceId, long id)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                var asignacion = await this._consultarRepository.ConsultarAsync<UsuarioRolEntity>(traceId, ur => ur.Id == id);
                if (asignacion is null)
                    return new NotFoundObjectResult(new RespuestaModel { Mensaje = MENSAJE_NO_ENCONTRADO, Exito = ERROR });

                await this._eliminarRepository.EliminarAsync<UsuarioRolEntity>(traceId, asignacion);

                return new OkObjectResult(new RespuestaModel { Mensaje = MENSAJE_ELIMINADO, Exito = EXITO });
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
