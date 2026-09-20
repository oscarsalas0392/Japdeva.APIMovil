using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.ListaRespuestaReclamoService;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerReclamosPorUsuarioOrdenadoService
{
    /// <summary>
    /// Servicio para obtener reclamos de un usuario ordenados por fecha de registro descendente.
    /// Retorna primero los reclamos más recientes.
    /// </summary>
    public class ObtenerReclamosPorUsuarioOrdenadoService : IObtenerReclamosPorUsuarioOrdenadoService
    {
        private readonly ILogger<ObtenerReclamosPorUsuarioOrdenadoService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IListaRespuestaReclamoService _listaRespuestaReclamoService;

        private const string MENSAJE_ERROR_ID_USUARIO_INVALIDO = "El idUsuario es inválido.";
        private const string MENSAJE_ERROR_ID_PAGINA = "La pagina es inválida, debe ser mayor a 0.";
        private const int ID_USUARIO_MINIMO = 1;
        private const int ID_PAGINA_MINIMO = 1;

        /// <summary>
        /// Inicializa una nueva instancia del servicio de obtención de reclamos por usuario ordenados.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos y errores.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        /// <param name="listaRespuestaReclamoService">Servicio para convertir entidades en modelos de respuesta.</param>
        public ObtenerReclamosPorUsuarioOrdenadoService(
            ILogger<ObtenerReclamosPorUsuarioOrdenadoService> logger,
            IServiceProvider serviceProvider,
            IListaRespuestaReclamoService listaRespuestaReclamoService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._listaRespuestaReclamoService = listaRespuestaReclamoService;
        }

        /// <summary>
        /// Obtiene una lista paginada de todos los reclamos de un usuario ordenados del más reciente al más antiguo.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idUsuario">Identificador del usuario externo propietario de los reclamos.</param>
        /// <param name="pagina">Número de página para la consulta paginada.</param>
        /// <returns>Lista paginada de todos los reclamos del usuario ordenados por fecha descendente.</returns>
        public async Task<IActionResult> ObtenerReclamosPorUsuarioOrdenadoAsync(string traceId, int idUsuario, int pagina)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                if (idUsuario < ID_USUARIO_MINIMO) throw new ArgumentException(MENSAJE_ERROR_ID_USUARIO_INVALIDO);
                if (pagina < ID_PAGINA_MINIMO) throw new ArgumentException(MENSAJE_ERROR_ID_PAGINA);

                using var scope = this._serviceProvider.CreateScope();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();

                var reclamos = await consultarListaRepository.ConsultarListaOrdenadaAsync<ReclamoEntity>(
                    traceId,
                    pagina,
                    q => q.OrderByDescending(r => r.FechaRegistro),
                    x => x.IdUsuarioExterno == idUsuario);

                List<ReclamoRespuestaModel> lista = await this._listaRespuestaReclamoService
                    .ObtenerListaRespuestaReclamoAsync(traceId, reclamos.Lista);

                var respuesta = new RespuestaListaModel<ReclamoRespuestaModel>();
                respuesta.TotalRegistros = reclamos.TotalRegistros;
                respuesta.CantidadPaginas = reclamos.CantidadPaginas;
                respuesta.PaginaActual = reclamos.PaginaActual;
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
