using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Common.Repositories.EliminarRepository;
using Japdeva.APIMovil.Usuarios.Entities;


namespace Japdeva.APIMovil.Usuarios.Services.EliminarUsuarioService
{
    /// <summary>
    /// Servicio para la eliminación de usuarios en el sistema.
    /// </summary>
    public class EliminarUsuarioService : IEliminarUsuarioService
    {
        private readonly ILogger<EliminarUsuarioService> _logger;
        private readonly IConsultarRepository _consultarRepository;
        private readonly IEliminarRepository _eliminarRepository;
        private const string MENSAJE_USUARIO_NO_ENCONTRADO = "Usuario no encontrado.";
        private const string MENSAJE_USUARIO_ELIMINADO = "Usuario eliminado correctamente.";
        private const bool EXITO = true;
        private const bool ERROR = false;


        /// <summary>
        /// Inicializa una nueva instancia de la clase EliminarUsuarioService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="consultarRepository">Repositorio para consultar entidades.</param>
        /// <param name="eliminarRepository">Repositorio para eliminar entidades.</param>
        public EliminarUsuarioService(ILogger<EliminarUsuarioService> logger, IConsultarRepository consultarRepository, IEliminarRepository eliminarRepository)
        {
            this._logger = logger;
            this._consultarRepository = consultarRepository ?? throw new ArgumentNullException(nameof(consultarRepository));
            this._eliminarRepository = eliminarRepository ?? throw new ArgumentNullException(nameof(eliminarRepository));
        }

        /// <summary>
        /// Elimina un usuario por su identificador.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="id">Identificador del usuario a eliminar</param>
        /// <returns>Resultado de la operación de eliminación</returns>
        public async Task<IActionResult> EliminarUsuarioAsync(string traceId, int id)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                var usuarioExistente = await this._consultarRepository.ConsultarAsync<UsuarioEntity>(traceId, u => u.Id == id);
                if (usuarioExistente is null)
                    return new NotFoundObjectResult(new RespuestaModel 
                    { 
                        Mensaje = MENSAJE_USUARIO_NO_ENCONTRADO, 
                        Exito = ERROR 
                    });

                var usuariosParaEliminar = new List<UsuarioEntity> { usuarioExistente };
                await this._eliminarRepository.EliminarAsync<UsuarioEntity>(traceId, usuariosParaEliminar);

                return new OkObjectResult(new RespuestaModel 
                { 
                    Mensaje = MENSAJE_USUARIO_ELIMINADO, 
                    Exito = EXITO 
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
