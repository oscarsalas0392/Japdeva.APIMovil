using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Usuarios.Entities;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.Services.ObtenerDepartamentosUsuariosService
{
    /// <summary>
    /// Servicio para consultar los departamentos asignados a un usuario.
    /// </summary>
    public class ObtenerDepartamentosUsuariosService : IObtenerDepartamentosUsuariosService
    {
        private readonly ILogger<ObtenerDepartamentosUsuariosService> _logger;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Inicializa una nueva instancia de ObtenerDepartamentosUsuariosService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        public ObtenerDepartamentosUsuariosService(ILogger<ObtenerDepartamentosUsuariosService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Retorna los departamentos asignados al usuario indicado.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="idUsuario">Identificador del usuario.</param>
        /// <param name="pagina">Número de página.</param>
        /// <returns>Lista paginada de asignaciones de departamentos.</returns>
        public async Task<IActionResult> ObtenerDepartamentosPorUsuarioAsync(string traceId, int idUsuario, int pagina)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                var resultadoConsulta = await consultarListaRepository.ConsultarListaAsync<DepartamentoUsuarioEntity>(traceId, pagina, asociacion => asociacion.IdUsuario == idUsuario && asociacion.Activo);
                var lista = new List<DepartamentoUsuarioRespuestaModel>();
                foreach (var item in resultadoConsulta.Lista)
                {
                    var modelo = new DepartamentoUsuarioRespuestaModel();
                    modelo.Id = item.Id;
                    modelo.IdUsuario = item.IdUsuario;
                    modelo.IdDepartamento = item.IdDepartamento;
                    modelo.IdUsuarioAdministrador = item.IdUsuarioAdministrador;
                    modelo.FechaRegistro = item.FechaRegistro;
                    modelo.Activo = item.Activo;
                    lista.Add(modelo);
                }
                var respuesta = new RespuestaListaModel<DepartamentoUsuarioRespuestaModel>();
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
