using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Reclamos.Services.AgregarReclamoDetalleService;
using Japdeva.APIMovil.Reclamos.Services.DevolucionProcesoCacheService;

namespace Japdeva.APIMovil.Reclamos.Services.AgregarDetalleReclamoPorDevolucionService
{
    /// <summary>
    /// Servicio encargado de agregar detalles de reclamo asociados a devoluciones de proceso.
    /// </summary>
    public class AgregarDetalleReclamoPorDevolucionService: IAgregarDetalleReclamoPorDevolucionService
    {
        private readonly ILogger<AgregarDetalleReclamoPorDevolucionService> _logger;
        private readonly IDevolucionProcesoCacheService _devolucionProcesoCacheService;
        private readonly IAgregarReclamoDetalleService _agregarReclamoDetalleService;

        private const string MENSAJE_ERROR_DEVOLUCION_PROCESO_NO_ENCONTRADO = "La devolución de proceso con Id {0} no fue encontrada.";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AgregarDetalleReclamoPorDevolucionService"/>.
        /// </summary>
        /// <param name="logger">Instancia de logger para registrar eventos.</param>
        /// <param name="devolucionProcesoCacheService">Servicio para obtener devoluciones de proceso desde caché.</param>
        /// <param name="agregarReclamoDetalleService">Servicio para agregar detalles de reclamo.</param>
        public AgregarDetalleReclamoPorDevolucionService(
            ILogger<AgregarDetalleReclamoPorDevolucionService> logger,
            IDevolucionProcesoCacheService devolucionProcesoCacheService,
            IAgregarReclamoDetalleService agregarReclamoDetalleService)
            => (this._logger, this._devolucionProcesoCacheService, this._agregarReclamoDetalleService) = (logger, devolucionProcesoCacheService, agregarReclamoDetalleService);

        /// <summary>
        /// Agrega un detalle de reclamo asociado a una devolución de proceso específica.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el registro de logs.</param>
        /// <param name="idReclamo">Identificador único del reclamo.</param>
        /// <param name="idDevolucionProceso">Identificador de la devolución de proceso.</param>
        public async Task AgregarDetalleReclamoPorDevolucionAsync(string traceId, long idReclamo, int idDevolucionProceso)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                var devolucionProceso = this._devolucionProcesoCacheService.ObtenerDevolucionProceso(traceId, idDevolucionProceso);
                if (devolucionProceso is null) throw new Exception(string.Format(MENSAJE_ERROR_DEVOLUCION_PROCESO_NO_ENCONTRADO, idDevolucionProceso));
                await this._agregarReclamoDetalleService.AgregarReclamoDetalleAsync(traceId, idReclamo, devolucionProceso.IdOrdenProceso);
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
