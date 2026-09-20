using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoCacheService;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoOrdenProcesoCacheService;
using Japdeva.APIMovil.Reclamos.Services.NivelProcesoCacheService;
using Japdeva.APIMovil.Reclamos.Services.ValidarEstadoDetalleApelacionReclamoService;

namespace Japdeva.APIMovil.Reclamos.Services.EditarApelacionReclamoDetalleService
{
    /// <summary>
    /// Servicio para editar detalles de apelación de reclamo.
    /// Actualiza el estado, descripción y usuario interno del detalle,
    /// e invoca el motor de workflow cuando se indica un nivel siguiente de proceso.
    /// </summary>
    public class EditarApelacionReclamoDetalleService : IEditarApelacionReclamoDetalleService
    {
        private readonly ILogger<EditarApelacionReclamoDetalleService> _logger;
        private readonly IEstadoDetalleReclamoCacheService _estadoDetalleReclamoCacheService;
        private readonly IEstadoDetalleReclamoOrdenProcesoCacheService _estadoDetalleReclamoOrdenProcesoCacheService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IValidarEstadoDetalleApelacionReclamoService _validarEstadoDetalleApelacionReclamoService;
        private readonly INivelProcesoCacheService _nivelProcesoCacheService;

        private const string MENSAJE_ERROR_DETALLE_NO_ENCONTRADO = "El detalle de apelación con Id {0} no fue encontrado.";
        private const string MENSAJE_ERROR_ESTADO_DETALLE_NO_ENCONTRADO = "El estado detalle con Id {0} no fue encontrado.";
        private const string MENSAJE_ERROR_NIVEL_PROCESO_NO_ENCONTRADO = "El nivel del proceso con Id {0} no fue encontrado.";
        private const string MENSAJE_ERROR_ESTADO_DETALLE_ORDEN_NO_ENCONTRADO = "No se encuentra el estado asociado al nivel de proceso con Id {0}.";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="EditarApelacionReclamoDetalleService"/>.
        /// </summary>
        public EditarApelacionReclamoDetalleService(
            ILogger<EditarApelacionReclamoDetalleService> logger,
            IServiceProvider serviceProvider,
            IEstadoDetalleReclamoCacheService estadoDetalleReclamoCacheService,
            IEstadoDetalleReclamoOrdenProcesoCacheService estadoDetalleReclamoOrdenProcesoCacheService,
            IValidarEstadoDetalleApelacionReclamoService validarEstadoDetalleApelacionReclamoService,
            INivelProcesoCacheService nivelProcesoCacheService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._estadoDetalleReclamoCacheService = estadoDetalleReclamoCacheService;
            this._estadoDetalleReclamoOrdenProcesoCacheService = estadoDetalleReclamoOrdenProcesoCacheService;
            this._validarEstadoDetalleApelacionReclamoService = validarEstadoDetalleApelacionReclamoService;
            this._nivelProcesoCacheService = nivelProcesoCacheService;
        }

        /// <summary>
        /// Edita un detalle de apelación existente con validaciones y control de workflow.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="solicitud">Modelo con los datos de la edición del detalle.</param>
        /// <returns>Resultado de la operación con la información del detalle editado.</returns>
        public async Task<IActionResult> EditarApelacionReclamoDetalleAsync(string traceId, EditarDetalleApelacionReclamoSolicitudModel solicitud)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            using var scope = this._serviceProvider.CreateScope();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var actualizarRepository = scope.ServiceProvider.GetRequiredService<IActualizarRepository>();

                var detalle = await consultarRepository.ConsultarAsync<DetalleApelacionReclamoEntity>(traceId, x => x.Id == solicitud.IdDetalleApelacionReclamo);
                if (detalle is null) throw new ArgumentException(string.Format(MENSAJE_ERROR_DETALLE_NO_ENCONTRADO, solicitud.IdDetalleApelacionReclamo));

                int? idNivelSiguienteProceso = null;
                if (solicitud.IdNivelSiguienteProceso is not null)
                {
                    var nivelSiguiente = this._nivelProcesoCacheService.ObtenerNivelProcesoPorId(traceId, solicitud.IdNivelSiguienteProceso.Value);
                    if (nivelSiguiente is null || nivelSiguiente.IdProceso != (int)ProcesoModel.Apelacion) throw new ArgumentException(string.Format(MENSAJE_ERROR_NIVEL_PROCESO_NO_ENCONTRADO, solicitud.IdNivelSiguienteProceso.Value));
                    idNivelSiguienteProceso = nivelSiguiente.Id;
                }

                var estadoDetalle = this._estadoDetalleReclamoCacheService.ObtenerEstadoDetalleReclamo(traceId, solicitud.IdEstadoDetalleReclamo);
                if (estadoDetalle is null) throw new ArgumentException(string.Format(MENSAJE_ERROR_ESTADO_DETALLE_NO_ENCONTRADO, solicitud.IdEstadoDetalleReclamo));

                var estadoDetalleOrden = this._estadoDetalleReclamoOrdenProcesoCacheService.ObtenerEstadoDetalleReclamoOrdenProceso(traceId, detalle.IdNivelProceso, solicitud.IdEstadoDetalleReclamo);
                if (estadoDetalleOrden is null) throw new ArgumentException(string.Format(MENSAJE_ERROR_ESTADO_DETALLE_ORDEN_NO_ENCONTRADO, detalle.IdNivelProceso));

                detalle.IdEstadoDetalleReclamo = solicitud.IdEstadoDetalleReclamo;
                detalle.FechaEdicion = DateTime.UtcNow;
                detalle.IdUsuarioInterno = solicitud.IdUsuarioInterno;

                int nivelProcesoActual = detalle.IdNivelProceso;
                long idApelacionReclamo = detalle.IdApelacionReclamo;
                string descripcionResolucion = solicitud.DescripcionResolucion;

                await actualizarRepository.ActualizarAsync<DetalleApelacionReclamoEntity>(traceId, detalle);

                if (idNivelSiguienteProceso is not null)
                {
                    await this._validarEstadoDetalleApelacionReclamoService.ValidarEstadoDetalleApelacionReclamoAsync(
                        traceId, estadoDetalle, idApelacionReclamo, nivelProcesoActual, idNivelSiguienteProceso.Value, descripcionResolucion);
                }

                EditarDetalleApelacionReclamoRespuestaModel respuesta = new EditarDetalleApelacionReclamoRespuestaModel();
                respuesta.Id = detalle.Id;
                respuesta.IdEstadoDetalleReclamo = detalle.IdEstadoDetalleReclamo;
                respuesta.FechaEdicion = detalle.FechaEdicion.Value;

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
