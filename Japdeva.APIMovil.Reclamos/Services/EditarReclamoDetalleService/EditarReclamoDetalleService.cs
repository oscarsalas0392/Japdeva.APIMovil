using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoCacheService;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoOrdenProcesoCacheService;
using Japdeva.APIMovil.Reclamos.Services.ManejarTransicionEstadoReclamoService;
using Japdeva.APIMovil.Reclamos.Services.NivelProcesoCacheService;

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
        private readonly INivelProcesoCacheService _nivelProcesoCacheService;
        private readonly IManejarTransicionEstadoReclamoService _manejarTransicionEstadoReclamoService;

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
        /// <param name="manejarTransicionEstadoReclamoService">Servicio para manejar la transición de estado del reclamo.</param>
        public EditarReclamoDetalleService(
            ILogger<EditarReclamoDetalleService> logger,
            IServiceProvider serviceProvider,
            IEstadoDetalleReclamoCacheService estadoDetalleReclamoCacheService,
            IEstadoDetalleReclamoOrdenProcesoCacheService estadoDetalleReclamoOrdenProcesoCacheService,
            INivelProcesoCacheService nivelProcesoCacheService,
            IManejarTransicionEstadoReclamoService manejarTransicionEstadoReclamoService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._estadoDetalleReclamoCacheService = estadoDetalleReclamoCacheService;
            this._estadoDetalleReclamoOrdenProcesoCacheService = estadoDetalleReclamoOrdenProcesoCacheService;
            this._nivelProcesoCacheService = nivelProcesoCacheService;
            this._manejarTransicionEstadoReclamoService = manejarTransicionEstadoReclamoService;
        }

        /// <summary>
        /// Edita un detalle de reclamo existente con validaciones y control transaccional.
        /// Delega la transición de estado del reclamo al servicio <see cref="IManejarTransicionEstadoReclamoService"/>.
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

                var reclamoDetalle = await consultarRepository.ConsultarAsync<DetalleReclamoEntity>(traceId, x => x.Id == editarDetalleReclamoSolicitudModel.IdDetalleReclamo);
                if (reclamoDetalle is null) throw new ArgumentException(string.Format(MENSAJE_ERROR_DETALLE_RECLAMO_NO_ENCONTRADO, editarDetalleReclamoSolicitudModel.IdDetalleReclamo));

                int? idNivelSiguienteProceso = this.ResolverNivelSiguiente(traceId, editarDetalleReclamoSolicitudModel.IdNivelSiguienteProceso);

                var estadoDetalle = this._estadoDetalleReclamoCacheService.ObtenerEstadoDetalleReclamo(traceId, editarDetalleReclamoSolicitudModel.IdEstadoDetalleReclamo);
                if (estadoDetalle is null) throw new ArgumentException(string.Format(MENSAJE_ERROR_ESTADO_DETALLE_RECLAMO_NO_ENCONTRADO, editarDetalleReclamoSolicitudModel.IdDetalleReclamo));

                var ordenProceso = this._estadoDetalleReclamoOrdenProcesoCacheService.ObtenerEstadoDetalleReclamoOrdenProceso(traceId, reclamoDetalle.IdNivelProceso, editarDetalleReclamoSolicitudModel.IdEstadoDetalleReclamo);
                if (ordenProceso is null) throw new ArgumentException(string.Format(MENSAJE_ERROR_ESTADO_DETALLE_ORDEN_RECLAMO_NO_ENCONTRADO, reclamoDetalle.IdNivelProceso));

                reclamoDetalle.IdEstadoDetalleReclamo = editarDetalleReclamoSolicitudModel.IdEstadoDetalleReclamo;
                reclamoDetalle.FechaEdicion = DateTime.UtcNow;
                reclamoDetalle.IdUsuarioInterno = editarDetalleReclamoSolicitudModel.IdUsuarioInterno;
                reclamoDetalle.Descripcion = editarDetalleReclamoSolicitudModel.Descripcion;
            

                int nivelProcesoActual = reclamoDetalle.IdNivelProceso;
                long idReclamo = reclamoDetalle.IdReclamo;

                await actualizarRepository.ActualizarAsync<DetalleReclamoEntity>(traceId, reclamoDetalle);

                string descripcionResolucion = editarDetalleReclamoSolicitudModel.DescripcionResolucion;
                await this._manejarTransicionEstadoReclamoService.
                    ManejarTransicionEstadoReclamoAsync(traceId, idReclamo, nivelProcesoActual, estadoDetalle, idNivelSiguienteProceso, descripcionResolucion);

                EditarDetalleReclamoRespuestaModel respuesta = new EditarDetalleReclamoRespuestaModel
                {
                    Id = idReclamo,
                    IdEstadoDetalleReclamo = reclamoDetalle.IdEstadoDetalleReclamo,
                    FechaEdicion = reclamoDetalle.FechaEdicion.Value
                };

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

        /// <summary>
        /// Resuelve y valida el ID del siguiente nivel de proceso, si fue proporcionado.
        /// </summary>
        /// <param name="traceId">Identificador de traza.</param>
        /// <param name="idNivelSiguiente">ID del siguiente nivel proporcionado en la solicitud.</param>
        /// <returns>El ID del siguiente nivel validado, o null si no se proporcionó.</returns>
        public int? ResolverNivelSiguiente(string traceId, int? idNivelSiguiente)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                if (idNivelSiguiente is null) return null;

                var nivelSiguiente = this._nivelProcesoCacheService.ObtenerNivelProcesoPorId(traceId, idNivelSiguiente.Value);
                if (nivelSiguiente is null || nivelSiguiente.IdProceso != (int)ProcesoModel.Reclamo)
                    throw new ArgumentException(string.Format(MENSAJE_ERROR_NIVEL_PROCESO_RECLAMO_NO_ENCONTRADO, idNivelSiguiente.Value));

                return nivelSiguiente.Id;
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
