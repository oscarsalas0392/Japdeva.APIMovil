using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.ListaRespuestaReclamoService;


namespace Japdeva.APIMovil.Reclamos.Services.ObtenerReclamoPorDepartamentoService
{
    /// <summary>
    /// Servicio para obtener reclamos por departamento.
    /// </summary>
    public class ObtenerReclamoPorDepartamentoService: IObtenerReclamoPorDepartamentoService
    {
        private readonly ILogger<ObtenerReclamoPorDepartamentoService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IListaRespuestaReclamoService _listaRespuestaReclamoService;

        private const string MENSAJE_ERROR_ID_PAGINA = "La pagina es inválida, debe ser mayor a 0.";
        private const int ID_PAGINA_MINIMO = 1;
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerReclamoPorDepartamentoService"/>.
        /// </summary>
        /// <param name="logger">Instancia de logger para la clase.</param>
        /// <param name="consultarListaRepository">Repositorio para consultar listas.</param>
        public ObtenerReclamoPorDepartamentoService(
            ILogger<ObtenerReclamoPorDepartamentoService> logger,
            IServiceProvider serviceProvider,
            IListaRespuestaReclamoService listaRespuestaReclamoService
            )
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._listaRespuestaReclamoService = listaRespuestaReclamoService;
        }

        /// <summary>
        /// Obtiene los reclamos asociados a un departamento específico de forma paginada.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idDepartamento">Identificador del departamento para filtrar los reclamos.</param>
        /// <param name="pagina">Número de página para la paginación de resultados.</param>
        /// <returns>Una acción HTTP que contiene la lista de reclamos encontrados.</returns>
        public async Task<IActionResult> ObtenerReclamosPorDepartamentoAsync(string traceId, int idDepartamento, int pagina)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                if (pagina < ID_PAGINA_MINIMO) throw new ArgumentException(MENSAJE_ERROR_ID_PAGINA);

                var reclamos = await consultarListaRepository.ConsultarListaAsync<ReclamoEntity>(traceId, pagina,
                    reclamo => reclamo.IdDepartamentoActual == idDepartamento);

                List<ReclamoRespuestaModel> listaReclamoRespuestas = await this._listaRespuestaReclamoService.ObtenerListaRespuestaReclamoAsync(traceId, reclamos.Lista);
                var respuesta = new RespuestaListaModel<ReclamoRespuestaModel>();
                respuesta.TotalRegistros = reclamos.TotalRegistros;
                respuesta.CantidadPaginas = reclamos.CantidadPaginas;
                respuesta.PaginaActual = reclamos.PaginaActual;
                respuesta.Lista = listaReclamoRespuestas;

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
