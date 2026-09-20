using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EditarReclamoService;
using Japdeva.APIMovil.Reclamos.Services.NivelProcesoCacheService;
using Japdeva.APIMovil.Reclamos.Services.ValidarEstadoDetalleReclamoService;

namespace Japdeva.APIMovil.Reclamos.Services.ManejarTransicionEstadoReclamoService
{
    /// <summary>
    /// Servicio que maneja la transición de estado del reclamo al editar un detalle.
    /// Determina si el reclamo debe cambiar a EnProceso (primer nivel) y delega
    /// la validación de avance al siguiente nivel cuando corresponde.
    /// </summary>
    public class ManejarTransicionEstadoReclamoService : IManejarTransicionEstadoReclamoService
    {
        private readonly ILogger<ManejarTransicionEstadoReclamoService> _logger;
        private readonly INivelProcesoCacheService _nivelProcesoCacheService;
        private readonly IEditarReclamoService _editarReclamoService;
        private readonly IValidarEstadoDetalleReclamoService _validarEstadoDetalleReclamoService;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ManejarTransicionEstadoReclamoService"/>.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="nivelProcesoCacheService">Servicio de caché de niveles de proceso.</param>
        /// <param name="editarReclamoService">Servicio para editar el estado del reclamo.</param>
        /// <param name="validarEstadoDetalleReclamoService">Servicio para validar el avance al siguiente nivel.</param>
        public ManejarTransicionEstadoReclamoService(
            ILogger<ManejarTransicionEstadoReclamoService> logger,
            INivelProcesoCacheService nivelProcesoCacheService,
            IEditarReclamoService editarReclamoService,
            IValidarEstadoDetalleReclamoService validarEstadoDetalleReclamoService)
        {
            this._logger = logger;
            this._nivelProcesoCacheService = nivelProcesoCacheService;
            this._editarReclamoService = editarReclamoService;
            this._validarEstadoDetalleReclamoService = validarEstadoDetalleReclamoService;
        }

        /// <summary>
        /// Maneja la transición de estado del reclamo al editar un detalle.
        /// Cambia el reclamo a EnProceso si se edita desde el primer nivel con ContinuaProceso,
        /// y delega la validación de avance al siguiente nivel cuando corresponde.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idReclamo">Identificador del reclamo.</param>
        /// <param name="nivelProcesoActual">Nivel de proceso actual del detalle editado.</param>
        /// <param name="estadoDetalle">Entidad del estado del detalle de reclamo.</param>
        /// <param name="idNivelSiguienteProceso">ID del siguiente nivel de proceso, o null si no aplica.</param>
        /// <param name="descripcionResolucion">Descripción de la resolución aplicada.</param>
        public async Task ManejarTransicionEstadoReclamoAsync(string traceId, long idReclamo, int nivelProcesoActual, EstadoDetalleReclamoEntity estadoDetalle, int? idNivelSiguienteProceso, string descripcionResolucion)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                var primerNivel = this._nivelProcesoCacheService.ObtenerPrimerNivelPorProceso(traceId, (int)ProcesoModel.Reclamo);
                bool esNivelInicial = primerNivel is not null && primerNivel.Id == nivelProcesoActual;

                if (esNivelInicial && estadoDetalle.ContinuaProceso)
                    await this._editarReclamoService.EditarReclamoAsync(traceId, idReclamo, string.Empty, (int)EstadoReclamoModel.EnProceso);

                if (idNivelSiguienteProceso is not null)
                {
                    int idNivelSiguiente = idNivelSiguienteProceso.Value;
                    Task tareaValidar = this._validarEstadoDetalleReclamoService.ValidarEstadoDetalleReclamoAsync(traceId, estadoDetalle, idReclamo, nivelProcesoActual, idNivelSiguiente, descripcionResolucion);
                    await tareaValidar;
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
