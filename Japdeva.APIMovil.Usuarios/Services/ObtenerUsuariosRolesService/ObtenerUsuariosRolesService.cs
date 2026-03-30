using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
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
        private readonly IConsultarListaRepository _consultarListaRepository;

        /// <summary>
        /// Inicializa una nueva instancia de ObtenerUsuariosRolesService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="consultarListaRepository">Repositorio para consultar listas de entidades.</param>
        public ObtenerUsuariosRolesService(ILogger<ObtenerUsuariosRolesService> logger, IConsultarListaRepository consultarListaRepository)
        {
            this._logger = logger;
            this._consultarListaRepository = consultarListaRepository ?? throw new ArgumentNullException(nameof(consultarListaRepository));
        }

        /// <summary>
        /// Retorna los roles asignados al usuario indicado.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="idUsuario">Identificador del usuario.</param>
        /// <param name="pagina">Número de página.</param>
        /// <returns>Lista paginada de asignaciones de roles.</returns>
        public async Task<IActionResult> ObtenerRolesPorUsuarioAsync(string traceId, int idUsuario, int pagina)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                Expression<Func<UsuarioRolEntity, bool>> filtro = ur => ur.IdUsuario == idUsuario && ur.Activo;
                var resultadoConsulta = await this._consultarListaRepository.ConsultarListaAsync<UsuarioRolEntity>(traceId, pagina, filtro);

                var lista = new List<UsuarioRolRespuestaModel>();
                foreach (var item in resultadoConsulta.Lista)
                {
                    var modelo = new UsuarioRolRespuestaModel();
                    modelo.Id = item.Id;
                    modelo.IdUsuario = item.IdUsuario;
                    modelo.IdRol = item.IdRol;
                    modelo.IdUsuarioAdministrador = item.IdUsuarioAdministrador;
                    modelo.FechaRegistro = item.FechaRegistro;
                    modelo.Activo = item.Activo;
                    lista.Add(modelo);
                }

                var respuesta = new RespuestaListaModel<UsuarioRolRespuestaModel>();
                respuesta.TotalRegistros = resultadoConsulta.TotalRegistros;
                respuesta.CantidadPaginas = resultadoConsulta.CantidadPaginas;
                respuesta.PaginaActual = resultadoConsulta.PaginaActual;
                respuesta.Lista = lista;

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
