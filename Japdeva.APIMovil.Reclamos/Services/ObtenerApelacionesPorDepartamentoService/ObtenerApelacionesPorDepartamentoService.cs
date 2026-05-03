using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.ListaRespuestaApelacionReclamoService;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerApelacionesPorDepartamentoService
{
    /// <summary>
    /// Servicio para obtener apelaciones activas asignadas a un departamento específico.
    /// Retorna únicamente apelaciones en estado Pendiente o EnProceso cuyo departamento actual coincida.
    /// </summary>
    public class ObtenerApelacionesPorDepartamentoService : IObtenerApelacionesPorDepartamentoService
    {
        private readonly ILogger<ObtenerApelacionesPorDepartamentoService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IListaRespuestaApelacionReclamoService _listaRespuestaApelacionReclamoService;

        private const string MENSAJE_ERROR_ID_PAGINA = "La pagina es inválida, debe ser mayor a 0.";
        private const int ID_PAGINA_MINIMO = 1;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerApelacionesPorDepartamentoService"/>.
        /// </summary>
        public ObtenerApelacionesPorDepartamentoService(
            ILogger<ObtenerApelacionesPorDepartamentoService> logger,
            IServiceProvider serviceProvider,
            IListaRespuestaApelacionReclamoService listaRespuestaApelacionReclamoService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._listaRespuestaApelacionReclamoService = listaRespuestaApelacionReclamoService;
        }

        /// <summary>
        /// Obtiene una lista paginada de apelaciones activas asignadas al departamento indicado.
        /// </summary>
        public async Task<IActionResult> ObtenerApelacionesPorDepartamentoAsync(string traceId, int idDepartamento, int pagina)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (pagina < ID_PAGINA_MINIMO) throw new ArgumentException(MENSAJE_ERROR_ID_PAGINA);

                using var scope = this._serviceProvider.CreateScope();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();

                var apelaciones = await consultarListaRepository.ConsultarListaAsync<ApelacionReclamoEntity>(traceId, pagina,
                    x => x.IdDepartamentoActual == idDepartamento
                      && (x.IdEstadoReclamo == (int)EstadoReclamoModel.Pendiente
                          || x.IdEstadoReclamo == (int)EstadoReclamoModel.EnProceso));

                List<ApelacionReclamoRespuestaModel> lista = await this._listaRespuestaApelacionReclamoService
                    .ObtenerListaRespuestaApelacionReclamoAsync(traceId, apelaciones.Lista);

                var respuesta = new RespuestaListaModel<ApelacionReclamoRespuestaModel>();
                respuesta.TotalRegistros = apelaciones.TotalRegistros;
                respuesta.CantidadPaginas = apelaciones.CantidadPaginas;
                respuesta.PaginaActual = apelaciones.PaginaActual;
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
