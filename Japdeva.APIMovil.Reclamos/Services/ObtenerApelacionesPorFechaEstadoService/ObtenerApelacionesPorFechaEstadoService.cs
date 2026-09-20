using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.ListaRespuestaApelacionReclamoService;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerApelacionesPorFechaEstadoService
{
    /// <summary>
    /// Servicio para obtener apelaciones filtradas por rango de fechas de ingreso y estado con paginación.
    /// </summary>
    public class ObtenerApelacionesPorFechaEstadoService : IObtenerApelacionesPorFechaEstadoService
    {
        private readonly ILogger<ObtenerApelacionesPorFechaEstadoService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IListaRespuestaApelacionReclamoService _listaRespuestaApelacionReclamoService;

        private const string MENSAJE_ERROR_ID_PAGINA = "La pagina es inválida, debe ser mayor a 0.";
        private const int ID_PAGINA_MINIMO = 1;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerApelacionesPorFechaEstadoService"/>.
        /// </summary>
        public ObtenerApelacionesPorFechaEstadoService(
            ILogger<ObtenerApelacionesPorFechaEstadoService> logger,
            IServiceProvider serviceProvider,
            IListaRespuestaApelacionReclamoService listaRespuestaApelacionReclamoService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._listaRespuestaApelacionReclamoService = listaRespuestaApelacionReclamoService;
        }

        /// <summary>
        /// Obtiene una lista paginada de apelaciones filtradas por rango de fechas de ingreso y estado.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="fechaInicio">Fecha de inicio del rango a consultar.</param>
        /// <param name="fechaFin">Fecha de fin del rango a consultar.</param>
        /// <param name="idEstadoReclamo">Identificador del estado a filtrar.</param>
        /// <param name="pagina">Número de página para la consulta paginada.</param>
        /// <returns>Lista paginada de apelaciones que cumplen los criterios.</returns>
        public async Task<IActionResult> ObtenerApelacionesPorFechaEstadoAsync(string traceId, DateTime fechaInicio, DateTime fechaFin, int idEstadoReclamo, int pagina)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (pagina < ID_PAGINA_MINIMO) throw new ArgumentException(MENSAJE_ERROR_ID_PAGINA);

                using var scope = this._serviceProvider.CreateScope();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();

                var apelaciones = await consultarListaRepository.ConsultarListaAsync<ApelacionReclamoEntity>(traceId, pagina,
                    x => x.FechaRegistro >= fechaInicio && x.FechaRegistro <= fechaFin && x.IdEstadoReclamo == idEstadoReclamo);

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
