using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Services.AgregarDetalleReclamoPorDevolucionService;
using Japdeva.APIMovil.Reclamos.Services.AgregarDetalleReclamoPorOrdenService;

namespace Japdeva.APIMovil.Reclamos.Services.ValidarEstadoDetalleReclamoService
{
    /// <summary>
    /// Servicio para validar el estado del detalle de un reclamo y ejecutar las acciones correspondientes
    /// según el proceso actual.
    /// </summary>
    public class ValidarEstadoDetalleReclamoService : IValidarEstadoDetalleReclamoService
    {
        private readonly ILogger<ValidarEstadoDetalleReclamoService> _logger;
        private readonly IAgregarDetalleReclamoPorOrdenService _agregarReclamoDetallePorOrdenService;
        private readonly IAgregarDetalleReclamoPorDevolucionService _agregarDetalleReclamoPorDevolucionService;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ValidarEstadoDetalleReclamoService"/>.
        /// </summary>
        public ValidarEstadoDetalleReclamoService(
            ILogger<ValidarEstadoDetalleReclamoService> logger,
            IAgregarDetalleReclamoPorOrdenService agregarReclamoDetallePorOrdenService,
            IAgregarDetalleReclamoPorDevolucionService agregarDetalleReclamoPorDevolucionService
            )
        {
            this._logger = logger;
            this._agregarReclamoDetallePorOrdenService = agregarReclamoDetallePorOrdenService;
            this._agregarDetalleReclamoPorDevolucionService = agregarDetalleReclamoPorDevolucionService;
        }

        /// <summary>
        /// Valida el estado del detalle de un reclamo y ejecuta las acciones correspondientes según el proceso actual.
        /// Agrega detalles por devolución u orden, o rechaza el proceso según los valores de la entidad.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el registro de logs.</param>
        /// <param name="estadoDetalleReclamoEntity">Entidad que representa el estado del detalle del reclamo.</param>
        /// <param name="idReclamo">Identificador único del reclamo.</param>
        /// <param name="idOrdenProcesoActual">Identificador del proceso de orden actual.</param>
        /// <param name="idDevolucionProceso">Identificador del proceso de devolución, si aplica.</param>
        public async Task ValidarEstadoDetalleReclamoAsync(string traceId, EstadoDetalleReclamoEntity estadoDetalleReclamoEntity, long idReclamo, int idOrdenProcesoActual, int? idDevolucionProceso)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (idDevolucionProceso is not null && estadoDetalleReclamoEntity.DevolucionProceso)
                {
                    await this._agregarDetalleReclamoPorDevolucionService.AgregarDetalleReclamoPorDevolucionAsync(traceId, idReclamo, idDevolucionProceso.Value);
                }

                if(estadoDetalleReclamoEntity.ContinuaProceso)
                { 
                    await this._agregarReclamoDetallePorOrdenService.AgregarDetalleReclamoPorOrdenAsync(traceId, idReclamo, idOrdenProcesoActual);
                }

                if(estadoDetalleReclamoEntity.RechazaProceso)
                {
                    // Lógica para rechazar el proceso del reclamo
                }
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
