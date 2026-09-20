using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoCacheService;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoOrdenProcesoCacheService;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerEstadoDetalleReclamoService
{
    /// <summary>
    /// Servicio para obtener el estado de detalle de un reclamo según el nivel de proceso.
    /// </summary>
    public class ObtenerEstadoDetalleReclamoService : IObtenerEstadoDetalleReclamoService
    {
        private readonly ILogger<ObtenerEstadoDetalleReclamoService> _logger;
        private readonly IEstadoDetalleReclamoCacheService _estadoDetalleReclamoCacheService;
        private readonly IEstadoDetalleReclamoOrdenProcesoCacheService _estadoDetalleReclamoOrdenProcesoCacheService;

        private const string MENSAJE_ERROR_ESTADO_DETALLE_RECLAMO_NO_EXISTE = "No se encontró el estado de detalle de reclamo en caché para el Id:{0}";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerEstadoDetalleReclamoService"/>.
        /// </summary>
        public ObtenerEstadoDetalleReclamoService(
            ILogger<ObtenerEstadoDetalleReclamoService> logger,
            IEstadoDetalleReclamoCacheService estadoDetalleReclamoCacheService,
            IEstadoDetalleReclamoOrdenProcesoCacheService estadoDetalleReclamoOrdenProcesoCacheService)
        {
            this._logger = logger;
            this._estadoDetalleReclamoCacheService = estadoDetalleReclamoCacheService;
            this._estadoDetalleReclamoOrdenProcesoCacheService = estadoDetalleReclamoOrdenProcesoCacheService;
        }

        /// <summary>
        /// Obtiene el estado de detalle de un reclamo según el nivel de proceso y la página solicitada.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idNivelProceso">Identificador del nivel de proceso del reclamo.</param>
        /// <param name="pagina">Número de página para la paginación de resultados.</param>
        /// <returns>Una acción de resultado que contiene la lista de estados de detalle del reclamo.</returns>
        public async Task<IActionResult> ObtenerEstadoDetalleReclamoAsync(string traceId, int idNivelProceso, int pagina)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                var estadosDetalle = this._estadoDetalleReclamoOrdenProcesoCacheService.ObtenerLista(traceId, pagina, reclamo => reclamo.IdNivelProceso == idNivelProceso);

                List<EstadoDetalleReclamoRespuestaModel> listaRespuesta = new List<EstadoDetalleReclamoRespuestaModel>();
                foreach (var estadoDetallePorNivelReclamo in estadosDetalle.Lista)
                {
                    var estadoReclamo = this._estadoDetalleReclamoCacheService.ObtenerEstadoDetalleReclamo(traceId, estadoDetallePorNivelReclamo.IdEstadoDetalleReclamo);
                    if (estadoReclamo is null) throw new Exception(string.Format(MENSAJE_ERROR_ESTADO_DETALLE_RECLAMO_NO_EXISTE, estadoDetallePorNivelReclamo.IdEstadoDetalleReclamo));

                    listaRespuesta.Add(new EstadoDetalleReclamoRespuestaModel
                    {
                        IdEstadoDetalleReclamo = estadoReclamo.Id,
                        DescripcionEstadoDetalleReclamo = estadoReclamo.Descripcion,
                        ContinuaProceso = estadoReclamo.ContinuaProceso,
                        RechazaProceso = estadoReclamo.RechazaProceso,
                        DevolucionProceso = estadoReclamo.DevolucionProceso,
                        FinalizarProceso = estadoReclamo.FinalizarProceso
                    });
                }

                RespuestaListaModel<EstadoDetalleReclamoRespuestaModel> respuestaListaModel = new RespuestaListaModel<EstadoDetalleReclamoRespuestaModel>();
                respuestaListaModel.TotalRegistros = estadosDetalle.TotalRegistros;
                respuestaListaModel.CantidadPaginas = estadosDetalle.CantidadPaginas;
                respuestaListaModel.PaginaActual = estadosDetalle.PaginaActual;
                respuestaListaModel.Lista = listaRespuesta;

                return new OkObjectResult(respuestaListaModel);
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
