using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Common.Repositories.GeneralRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoCacheService;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoOrdenProcesoCacheService;
using Japdeva.APIMovil.Reclamos.Services.NivelProcesoCacheService;
using Japdeva.APIMovil.Reclamos.Services.ValidarEstadoDetalleReclamoService;

namespace Japdeva.APIMovil.Reclamos.Services.EditarReclamoDetalleService
{
    /// <summary>
    /// Servicio para editar detalles de reclamos.
    /// Proporciona funcionalidades para modificar el estado, descripción y otros atributos
    /// de los detalles de reclamos existentes con validaciones y control transaccional.
    /// </summary>
    public class EditarReclamoDetalleService : IEditarReclamoDetalleService
    {
        private readonly ILogger<EditarReclamoDetalleService> _logger;
        private readonly IEstadoDetalleReclamoCacheService _estadoDetalleReclamoCacheService;
        private readonly IEstadoDetalleReclamoOrdenProcesoCacheService _estadoDetalleReclamoOrdenProcesoCacheService;
        private readonly IConsultarRepository _consultarRepository;
        private readonly IActualizarRepository _actualizarRepository;
        private readonly IValidarEstadoDetalleReclamoService _validarEstadoDetalleReclamoService;
        private readonly IGeneralRepository _generalRepository;
        private readonly INivelProcesoCacheService _nivelProcesoCacheService;

        private const string MENSAJE_ERROR_DETALLE_RECLAMO_NO_ENCONTRADO = "El detalle de reclamo con Id {0} no fue encontrado.";
        private const string MENSAJE_ERROR_ESTADO_DETALLE_RECLAMO_NO_ENCONTRADO = "El estado detalle de reclamo con Id {0} no fue encontrado.";
        private const string MENSAJE_ERROR_NIVEL_PROCESO_RECLAMO_NO_ENCONTRADO = "La nivel del proceso de reclamo con Id {0} no fue encontrado.";
        private const string MENSAJE_ERROR_ESTADO_DETALLE_ORDEN_RECLAMO_NO_ENCONTRADO = "No se encuentra el estado asociado a la orden del detalle de reclamo con Id {0} no fue encontrado.";

        /// <summary>
        /// Inicializa una nueva instancia del servicio de edición de detalles de reclamo.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos y errores.</param>
        /// <param name="consultarRepository">Repositorio para operaciones de consulta.</param>
        /// <param name="actualizarRepository">Repositorio para operaciones de actualización.</param>
        /// <param name="estadoDetalleReclamoCacheService">Servicio de cache para estados de detalle de reclamo.</param>
        /// <param name="estadoDetalleReclamoOrdenProcesoCacheService">Servicio de cache para estado-orden de procesos.</param>
        /// <param name="devolucionProcesoCacheService">Servicio de cache para procesos de devolución.</param>
        /// <param name="validarEstadoDetalleReclamoService">Servicio de validación de estados de detalle.</param>
        /// <param name="generalRepository">Repositorio para operaciones generales y transacciones.</param>
        public EditarReclamoDetalleService(ILogger<EditarReclamoDetalleService> logger,
            IConsultarRepository consultarRepository,
            IActualizarRepository actualizarRepository,
            IEstadoDetalleReclamoCacheService estadoDetalleReclamoCacheService,
            IEstadoDetalleReclamoOrdenProcesoCacheService estadoDetalleReclamoOrdenProcesoCacheService,
            IValidarEstadoDetalleReclamoService validarEstadoDetalleReclamoService,
            IGeneralRepository generalRepository,
            INivelProcesoCacheService nivelProcesoCacheService
            )
        {
            this._logger = logger;
            this._consultarRepository = consultarRepository;
            this._actualizarRepository = actualizarRepository;
            this._estadoDetalleReclamoCacheService = estadoDetalleReclamoCacheService;
            this._estadoDetalleReclamoOrdenProcesoCacheService = estadoDetalleReclamoOrdenProcesoCacheService;
            this._nivelProcesoCacheService = nivelProcesoCacheService;
            this._generalRepository = generalRepository;
            this._validarEstadoDetalleReclamoService = validarEstadoDetalleReclamoService;
        }

        /// <summary>
        /// Edita un detalle de reclamo existente con validaciones y control transaccional.
        /// Actualiza el estado, descripción, proceso de devolución y usuario interno asignado.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="editarDetalleReclamoSolicitudModel">Modelo con los datos de la edición del detalle.</param>
        /// <returns>Resultado de la operación con la información del detalle editado.</returns>
        public async Task<IActionResult> EditarReclamoDetalleAsync(string traceId, EditarDetalleReclamoSolicitudModel editarDetalleReclamoSolicitudModel)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            IDbContextTransaction? transaccion = null;
            try
            {
                transaccion = await this._generalRepository.ObtenerTransaccionBaseDatosAsync(traceId);
                this._logger.Inicio(traceId, nombreMetodo);

                var reclamoDetalle = await this._consultarRepository.ConsultarAsync<DetalleReclamoEntity>(traceId, x=>x.Id == editarDetalleReclamoSolicitudModel.Id);
                if (reclamoDetalle is null) throw new ArgumentException(string.Format(MENSAJE_ERROR_DETALLE_RECLAMO_NO_ENCONTRADO, editarDetalleReclamoSolicitudModel.Id));

                var nivelSiguienteProceso = this._nivelProcesoCacheService.ObtenerNivelProcesoPorId(traceId, editarDetalleReclamoSolicitudModel.IdNivelSiguienteProceso);
                if (nivelSiguienteProceso is null) throw new ArgumentException(string.Format(MENSAJE_ERROR_NIVEL_PROCESO_RECLAMO_NO_ENCONTRADO, reclamoDetalle.IdNivelProceso));

                var estadoEstadoDetalleReclamo = this._estadoDetalleReclamoCacheService.ObtenerEstadoDetalleReclamo(traceId, editarDetalleReclamoSolicitudModel.IdEstadoDetalleReclamo);
                if(estadoEstadoDetalleReclamo is null) throw new ArgumentException(string.Format(MENSAJE_ERROR_ESTADO_DETALLE_RECLAMO_NO_ENCONTRADO, editarDetalleReclamoSolicitudModel.Id));

                var estadoDetalleReclamoOrdenProceso = this._estadoDetalleReclamoOrdenProcesoCacheService.ObtenerEstadoDetalleReclamoOrdenProceso(traceId, reclamoDetalle.IdNivelProceso, editarDetalleReclamoSolicitudModel.IdEstadoDetalleReclamo);
                if(estadoDetalleReclamoOrdenProceso is null) throw new ArgumentException(string.Format(MENSAJE_ERROR_ESTADO_DETALLE_ORDEN_RECLAMO_NO_ENCONTRADO, reclamoDetalle.IdNivelProceso));

                reclamoDetalle.IdEstadoDetalleReclamo = editarDetalleReclamoSolicitudModel.IdEstadoDetalleReclamo;
                reclamoDetalle.FechaEdicion = DateTime.Now;
                reclamoDetalle.IdUsuarioInterno = editarDetalleReclamoSolicitudModel.IdUsuarioInterno;

                string descripcionResolucion = editarDetalleReclamoSolicitudModel.DescripcionResolucion;
                int nivelProcesoActual = reclamoDetalle.IdNivelProceso;
                int nivelProcesoSiguiente = nivelSiguienteProceso.Id;
                long idReclamo = reclamoDetalle.IdReclamo;
                await this._actualizarRepository.ActualizarAsync<DetalleReclamoEntity>(traceId, reclamoDetalle);
                await this._validarEstadoDetalleReclamoService.ValidarEstadoDetalleReclamoAsync(traceId, estadoEstadoDetalleReclamo, idReclamo, 
                    nivelProcesoActual, nivelProcesoSiguiente, descripcionResolucion);

                await this._generalRepository.RealizarCommitBaseDatosAsync(traceId, transaccion);

                EditarDetalleReclamoRespuestaModel editarDetalleReclamoRespuestaModel = new EditarDetalleReclamoRespuestaModel();
                editarDetalleReclamoRespuestaModel.Id = idReclamo;
                editarDetalleReclamoRespuestaModel.IdEstadoDetalleReclamo = reclamoDetalle.IdEstadoDetalleReclamo;
                editarDetalleReclamoRespuestaModel.FechaEdicion = reclamoDetalle.FechaEdicion.Value;

                return new OkObjectResult(editarDetalleReclamoRespuestaModel);

            }
            catch (Exception ex)
            {

                if(transaccion is not null) await this._generalRepository.RealizarDevolucionCambiosBaseDatosAsync(traceId, transaccion);
                this._logger.Error(traceId, nombreMetodo, ex);
                throw;
            }
            finally 
            {
                await this._generalRepository.LimpiarTransaccionAsync(traceId, transaccion);
                this._logger.Fin(traceId, nombreMetodo);
            }
        }

    }
}
