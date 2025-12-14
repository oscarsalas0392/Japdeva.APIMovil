using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Services.AgregarReclamoDetalleService;
using Japdeva.APIMovil.Reclamos.Services.OrdenNivelProcesoCacheService;

namespace Japdeva.APIMovil.Reclamos.Services.ValidarEstadoDetalleReclamoService
{
    /// <summary>
    /// Servicio para validar el estado del detalle de un reclamo y ejecutar las acciones correspondientes
    /// según el proceso actual.
    /// </summary>
    public class ValidarEstadoDetalleReclamoService : IValidarEstadoDetalleReclamoService
    {
        private readonly ILogger<ValidarEstadoDetalleReclamoService> _logger;
        private readonly IAgregarReclamoDetalleService _agregarReclamoDetalleService;
        private readonly IOrdenNivelProcesoCacheService _ordenNivelProcesoCacheService;

        private const string MENSAJE_ERROR_ORDEN_NIVEL_PROCESO_NO_ENCONTRADO = "No se encontró la configuración de orden de nivel de proceso para IdNivelSuperior: {0} e IdNivelInferior: {1}";
        private const string MENSAJE_ERROR_ORDEN_NIVEL_PROCESO_NO_ES_DEVOLUCION = "No se encontró la configuración de devolución del orden de nivel de proceso para IdNivelSuperior: {0} e IdNivelInferior: {1}";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ValidarEstadoDetalleReclamoService"/>.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos y errores.</param>
        /// <param name="agregarReclamoDetalleService">Servicio para agregar detalles de reclamo.</param>
        public ValidarEstadoDetalleReclamoService(
            ILogger<ValidarEstadoDetalleReclamoService> logger,
            IAgregarReclamoDetalleService agregarReclamoDetalleService,
            IOrdenNivelProcesoCacheService ordenNivelProcesoCacheService
            )
        {
            this._logger = logger;
            this._agregarReclamoDetalleService = agregarReclamoDetalleService;
            this._ordenNivelProcesoCacheService = ordenNivelProcesoCacheService;
        }

        /// <summary>
        /// Valida el estado del detalle de un reclamo y ejecuta las acciones correspondientes según el proceso actual.
        /// Si el estado indica rechazo o finalización, no realiza ninguna acción adicional.
        /// Si el estado indica devolución, valida la configuración de devolución en el orden de nivel de proceso.
        /// Finalmente, agrega un nuevo detalle de reclamo para el siguiente nivel de proceso.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="estadoDetalle">Entidad que representa el estado del detalle del reclamo.</param>
        /// <param name="idReclamo">Identificador del reclamo.</param>
        /// <param name="idNivelActual">Identificador del nivel actual del proceso.</param>
        /// <param name="idNivelSiguienteProceso">Identificador del siguiente nivel del proceso.</param>

        public async Task ValidarEstadoDetalleReclamoAsync(string traceId, EstadoDetalleReclamoEntity estadoDetalle, long idReclamo, int idNivelActual, int idNivelSiguienteProceso)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                if (estadoDetalle.RechazaProceso)
                {
                    return;
                }

                if (estadoDetalle.FinalizarProceso)
                {
                    return;
                }

                if (estadoDetalle.DevolucionProceso)
                {
                    var ordenNivelProceso = this._ordenNivelProcesoCacheService.ObtenerOrdenNivelProcesoCache(traceId, idNivelActual, idNivelSiguienteProceso);
                    if(ordenNivelProceso is null) throw new Exception(string.Format(MENSAJE_ERROR_ORDEN_NIVEL_PROCESO_NO_ENCONTRADO, idNivelActual, idNivelSiguienteProceso));
                    if(!ordenNivelProceso.DevolucionNivel) throw new Exception(string.Format(MENSAJE_ERROR_ORDEN_NIVEL_PROCESO_NO_ES_DEVOLUCION, idNivelActual, idNivelSiguienteProceso));
                }

                await this._agregarReclamoDetalleService.AgregarReclamoDetalleAsync(traceId, idReclamo, idNivelSiguienteProceso);               
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
