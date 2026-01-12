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
        private readonly IServiceProvider _serviceProvider;
        private readonly IValidarEstadoDetalleReclamoService _validarEstadoDetalleReclamoService;
        private readonly INivelProcesoCacheService _nivelProcesoCacheService;

        private const string MENSAJE_ERROR_DETALLE_RECLAMO_NO_ENCONTRADO = "El detalle de reclamo con Id {0} no fue encontrado.";
        private const string MENSAJE_ERROR_ESTADO_DETALLE_RECLAMO_NO_ENCONTRADO = "El estado detalle de reclamo con Id {0} no fue encontrado.";
        private const string MENSAJE_ERROR_NIVEL_PROCESO_RECLAMO_NO_ENCONTRADO = "La nivel del proceso de reclamo con Id {0} no fue encontrado.";
        private const string MENSAJE_ERROR_ESTADO_DETALLE_ORDEN_RECLAMO_NO_ENCONTRADO = "No se encuentra el estado asociado a la orden del detalle de reclamo con Id {0} no fue encontrado.";


        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="EditarReclamoDetalleService"/>.
        /// </summary>
        /// <param name="logger">Instancia del registrador de logs.</param>
        /// <param name="serviceProvider">Proveedor de servicios para la obtención de dependencias.</param>
        /// <param name="estadoDetalleReclamoCacheService">Servicio de caché para estados de detalle de reclamo.</param>
        /// <param name="estadoDetalleReclamoOrdenProcesoCacheService">Servicio de caché para estados de detalle de reclamo por orden de proceso.</param>
        /// <param name="validarEstadoDetalleReclamoService">Servicio para validar el estado del detalle de reclamo.</param>
        /// <param name="nivelProcesoCacheService">Servicio de caché para niveles de proceso.</param>
        public EditarReclamoDetalleService(
            ILogger<EditarReclamoDetalleService> logger,
            IServiceProvider serviceProvider,
            IEstadoDetalleReclamoCacheService estadoDetalleReclamoCacheService,
            IEstadoDetalleReclamoOrdenProcesoCacheService estadoDetalleReclamoOrdenProcesoCacheService,
            IValidarEstadoDetalleReclamoService validarEstadoDetalleReclamoService,
            INivelProcesoCacheService nivelProcesoCacheService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._estadoDetalleReclamoCacheService = estadoDetalleReclamoCacheService;
            this._estadoDetalleReclamoOrdenProcesoCacheService = estadoDetalleReclamoOrdenProcesoCacheService;
            this._nivelProcesoCacheService = nivelProcesoCacheService;
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
            using var scope = this._serviceProvider.CreateScope();
            try
            {
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var actualizarRepository = scope.ServiceProvider.GetRequiredService<IActualizarRepository>();
                this._logger.Inicio(traceId, nombreMetodo);

                var reclamoDetalle = await consultarRepository.ConsultarAsync<DetalleReclamoEntity>(traceId, x=>x.Id == editarDetalleReclamoSolicitudModel.IdDetalleReclamo);
                if (reclamoDetalle is null) throw new ArgumentException(string.Format(MENSAJE_ERROR_DETALLE_RECLAMO_NO_ENCONTRADO, editarDetalleReclamoSolicitudModel.IdDetalleReclamo));

                int? idNivelSiguienteProceso = null;

                if (editarDetalleReclamoSolicitudModel.IdNivelSiguienteProceso is not null)
                {
                    var nivelSiguienteProceso = this._nivelProcesoCacheService.ObtenerNivelProcesoPorId(traceId, editarDetalleReclamoSolicitudModel.IdNivelSiguienteProceso.Value);
                    if (nivelSiguienteProceso is null) throw new ArgumentException(string.Format(MENSAJE_ERROR_NIVEL_PROCESO_RECLAMO_NO_ENCONTRADO, reclamoDetalle.IdNivelProceso));
                    idNivelSiguienteProceso = nivelSiguienteProceso.Id;
                }

                var estadoEstadoDetalleReclamo = this._estadoDetalleReclamoCacheService.ObtenerEstadoDetalleReclamo(traceId, editarDetalleReclamoSolicitudModel.IdEstadoDetalleReclamo);
                if(estadoEstadoDetalleReclamo is null) throw new ArgumentException(string.Format(MENSAJE_ERROR_ESTADO_DETALLE_RECLAMO_NO_ENCONTRADO, editarDetalleReclamoSolicitudModel.IdDetalleReclamo));

                var estadoDetalleReclamoOrdenProceso = this._estadoDetalleReclamoOrdenProcesoCacheService.ObtenerEstadoDetalleReclamoOrdenProceso(traceId, reclamoDetalle.IdNivelProceso, editarDetalleReclamoSolicitudModel.IdEstadoDetalleReclamo);
                if(estadoDetalleReclamoOrdenProceso is null) throw new ArgumentException(string.Format(MENSAJE_ERROR_ESTADO_DETALLE_ORDEN_RECLAMO_NO_ENCONTRADO, reclamoDetalle.IdNivelProceso));

                reclamoDetalle.IdEstadoDetalleReclamo = editarDetalleReclamoSolicitudModel.IdEstadoDetalleReclamo;
                reclamoDetalle.FechaEdicion = DateTime.Now;
                reclamoDetalle.IdUsuarioInterno = editarDetalleReclamoSolicitudModel.IdUsuarioInterno;

                string descripcionResolucion = editarDetalleReclamoSolicitudModel.DescripcionResolucion;
                int nivelProcesoActual = reclamoDetalle.IdNivelProceso;
           
                long idReclamo = reclamoDetalle.IdReclamo;
                await actualizarRepository.ActualizarAsync<DetalleReclamoEntity>(traceId, reclamoDetalle);


                if (idNivelSiguienteProceso is not null)
                {
                    await this._validarEstadoDetalleReclamoService.ValidarEstadoDetalleReclamoAsync(traceId, estadoEstadoDetalleReclamo, idReclamo,
                        nivelProcesoActual, idNivelSiguienteProceso.Value, descripcionResolucion);
                }

                EditarDetalleReclamoRespuestaModel editarDetalleReclamoRespuestaModel = new EditarDetalleReclamoRespuestaModel();
                editarDetalleReclamoRespuestaModel.Id = idReclamo;
                editarDetalleReclamoRespuestaModel.IdEstadoDetalleReclamo = reclamoDetalle.IdEstadoDetalleReclamo;
                editarDetalleReclamoRespuestaModel.FechaEdicion = reclamoDetalle.FechaEdicion.Value;

                return new OkObjectResult(editarDetalleReclamoRespuestaModel);

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
