using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Reclamos.Services.AgregarReclamoDetalleService;
using Japdeva.APIMovil.Reclamos.Services.OrdenProcesoCacheService;

namespace Japdeva.APIMovil.Reclamos.Services.AgregarDetalleReclamoPorOrdenService
{
    /// <summary>
    /// Servicio para agregar un detalle de reclamo asociado al siguiente proceso de orden,
    /// basado en el orden de proceso actual.
    /// </summary>
    public class AgregarDetalleReclamoPorOrdenService : IAgregarDetalleReclamoPorOrdenService
    {
        private readonly ILogger<AgregarDetalleReclamoPorOrdenService> _logger;
        private readonly IOrdenProcesoCacheService _ordenProcesoCacheService;
        private readonly IAgregarReclamoDetalleService _agregarReclamoDetalleService;

        private const string MENSAJE_ERROR_ORDEN_PROCESO_NO_ENCONTRADO = "El orden de proceso con Id {0} no fue encontrado.";
        private const int INCREMENTO_ORDEN_PROCESO = 1;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AgregarDetalleReclamoPorOrdenService"/>.
        /// </summary>
        public AgregarDetalleReclamoPorOrdenService(
            ILogger<AgregarDetalleReclamoPorOrdenService> logger,
            IOrdenProcesoCacheService ordenProcesoCacheService,
            IAgregarReclamoDetalleService agregarReclamoDetalleService)
        {
            this._logger = logger;
            this._ordenProcesoCacheService = ordenProcesoCacheService;
            this._agregarReclamoDetalleService = agregarReclamoDetalleService;
        }

        /// <summary>
        /// Agrega un detalle de reclamo asociado al siguiente proceso de orden, basado en el orden de proceso actual.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el registro de logs.</param>
        /// <param name="idReclamo">Identificador del reclamo al que se agregará el detalle.</param>
        /// <param name="idOrdenProcesoActual">Identificador del proceso de orden actual.</param>
        public async Task AgregarDetalleReclamoPorOrdenAsync(string traceId, long idReclamo, int idOrdenProcesoActual)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                var ordenProceso = this._ordenProcesoCacheService.ObtenerOrdenProcesoPorId(traceId, idOrdenProcesoActual);
                if (ordenProceso is null) throw new Exception(string.Format(MENSAJE_ERROR_ORDEN_PROCESO_NO_ENCONTRADO, idOrdenProcesoActual));
                var siguienteProceso = this._ordenProcesoCacheService.ObtenerOrdenProcesoPorOrden(traceId, ordenProceso.Orden + INCREMENTO_ORDEN_PROCESO);
                if (siguienteProceso is null) throw new Exception(string.Format(MENSAJE_ERROR_ORDEN_PROCESO_NO_ENCONTRADO, idOrdenProcesoActual));
                await this._agregarReclamoDetalleService.AgregarReclamoDetalleAsync(traceId, idReclamo, siguienteProceso.Id);
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
