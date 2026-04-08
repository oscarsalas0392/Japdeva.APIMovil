using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.AgregarReclamoDetalleService;
using Japdeva.APIMovil.Reclamos.Services.EditarDepartamentoReclamoService;
using Japdeva.APIMovil.Reclamos.Services.EditarReclamoService;
using Japdeva.APIMovil.Reclamos.Services.NotificarDepartamentoService;
using Japdeva.APIMovil.Reclamos.Services.NotificarResolucionUsuarioService;
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
        private readonly IEditarReclamoService _editarReclamoService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEditarDepartamentoReclamoService _editarDepartamentoReclamoService;
        private readonly INotificarDepartamentoService _notificarDepartamentoService;
        private readonly INotificarResolucionUsuarioService _notificarResolucionUsuarioService;

        private const string ESTADO_RECHAZADO = nameof(EstadoReclamoModel.Rechazado);
        private const string ESTADO_COMPLETADO = nameof(EstadoReclamoModel.Completado);
        private const string MENSAJE_ERROR_ORDEN_NIVEL_PROCESO_NO_ENCONTRADO = "No se encontró la configuración de orden de nivel de proceso para IdNivelSuperior: {0} e IdNivelInferior: {1}";
        private const string MENSAJE_ERROR_ORDEN_NIVEL_PROCESO_NO_ES_DEVOLUCION = "No se encontró la configuración de devolución del orden de nivel de proceso para IdNivelSuperior: {0} e IdNivelInferior: {1}";
        private const string MENSAJE_ERROR_DESCRIPCION_RESOLUCION_OBLIGATORIA = "La descripción de la resolución es obligatoria";
        private const string MENSAJE_ERROR_NIVEL_PROCESO_NO_ENCONTRADO = "El nivel proceso con Id {0} no fue encontrado.";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ValidarEstadoDetalleReclamoService"/>.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos y errores.</param>
        /// <param name="agregarReclamoDetalleService">Servicio para agregar detalles de reclamo.</param>
        /// <param name="ordenNivelProcesoCacheService">Servicio de caché para orden de nivel de proceso.</param>
        /// <param name="editarReclamoService">Servicio para editar el estado del reclamo.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        /// <param name="editarDepartamentoReclamoService">Servicio para actualizar el departamento del reclamo.</param>
        /// <param name="notificarDepartamentoService">Servicio para notificar al nuevo departamento asignado.</param>
        /// <param name="notificarResolucionUsuarioService">Servicio para notificar al usuario la resolución del reclamo.</param>
        public ValidarEstadoDetalleReclamoService(
            ILogger<ValidarEstadoDetalleReclamoService> logger,
            IAgregarReclamoDetalleService agregarReclamoDetalleService,
            IOrdenNivelProcesoCacheService ordenNivelProcesoCacheService,
            IEditarReclamoService editarReclamoService,
            IServiceProvider serviceProvider,
            IEditarDepartamentoReclamoService editarDepartamentoReclamoService,
            INotificarDepartamentoService notificarDepartamentoService,
            INotificarResolucionUsuarioService notificarResolucionUsuarioService)
        {
            this._logger = logger;
            this._agregarReclamoDetalleService = agregarReclamoDetalleService;
            this._ordenNivelProcesoCacheService = ordenNivelProcesoCacheService;
            this._editarReclamoService = editarReclamoService;
            this._serviceProvider = serviceProvider;
            this._editarDepartamentoReclamoService = editarDepartamentoReclamoService;
            this._notificarDepartamentoService = notificarDepartamentoService;
            this._notificarResolucionUsuarioService = notificarResolucionUsuarioService;
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
        /// <param name="descripcionResolucion">Descripción de la resolución aplicada al reclamo.</param>
        public async Task ValidarEstadoDetalleReclamoAsync(string traceId, EstadoDetalleReclamoEntity estadoDetalle, long idReclamo, int idNivelActual, int idNivelSiguienteProceso, string descripcionResolucion)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                if (estadoDetalle.RechazaProceso)
                {
                    if(string.IsNullOrWhiteSpace(descripcionResolucion)) throw new ArgumentException(MENSAJE_ERROR_DESCRIPCION_RESOLUCION_OBLIGATORIA);
                    await this._editarReclamoService.EditarReclamoAsync(traceId, idReclamo, descripcionResolucion, (int)EstadoReclamoModel.Rechazado);
                    this.DispararNotificacionResolucion(traceId, idReclamo, descripcionResolucion, ESTADO_RECHAZADO);
                    return;
                }

                if (estadoDetalle.FinalizarProceso)
                {
                    if (string.IsNullOrWhiteSpace(descripcionResolucion)) throw new ArgumentException(MENSAJE_ERROR_DESCRIPCION_RESOLUCION_OBLIGATORIA);
                    await this._editarReclamoService.EditarReclamoAsync(traceId, idReclamo, descripcionResolucion, (int)EstadoReclamoModel.Completado);
                    this.DispararNotificacionResolucion(traceId, idReclamo, descripcionResolucion, ESTADO_COMPLETADO);
                    return;
                }

                using var scope = this._serviceProvider.CreateScope();

                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var nivelProceso = await consultarRepository.ConsultarAsync<NivelProcesoEntity>(traceId, x => x.Id == idNivelSiguienteProceso);
                if(nivelProceso is null) throw new Exception(string.Format(MENSAJE_ERROR_NIVEL_PROCESO_NO_ENCONTRADO, idNivelSiguienteProceso));

                if (estadoDetalle.DevolucionProceso)
                {
                    var ordenNivelProceso = this._ordenNivelProcesoCacheService.ObtenerOrdenNivelProcesoCache(traceId, idNivelActual, nivelProceso.Id);
                    if(ordenNivelProceso is null) throw new Exception(string.Format(MENSAJE_ERROR_ORDEN_NIVEL_PROCESO_NO_ENCONTRADO, idNivelActual, nivelProceso.Id));
                    if(!ordenNivelProceso.DevolucionNivel) throw new Exception(string.Format(MENSAJE_ERROR_ORDEN_NIVEL_PROCESO_NO_ES_DEVOLUCION, idNivelActual, nivelProceso.Id));
                }

                Task tareaAgregarReclamo = this._agregarReclamoDetalleService.AgregarReclamoDetalleAsync(traceId, idReclamo, idNivelSiguienteProceso);
                Task tareaActualizarDepartamentoReclamo = this._editarDepartamentoReclamoService.EditarDepartamentoReclamoAsync(traceId, idReclamo, nivelProceso.IdDepartamento);
                await Task.WhenAll(tareaAgregarReclamo, tareaActualizarDepartamentoReclamo);

                long idDepartamento = nivelProceso.IdDepartamento;
                _ = Task.Run(async () =>
                {
                    var reclamo = await consultarRepository.ConsultarAsync<ReclamoEntity>(traceId, r => r.Id == idReclamo);
                    if (reclamo is not null)
                        await this._notificarDepartamentoService.NotificarNuevoReclamoAsync(traceId, idDepartamento, idReclamo, reclamo.IdUsuarioExterno);
                });
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
        /// Dispara en segundo plano la notificación de resolución al usuario propietario del reclamo.
        /// Consulta el usuario asociado al reclamo y envía el correo con el estado y descripción de la resolución.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="idReclamo">Identificador del reclamo resuelto.</param>
        /// <param name="descripcionResolucion">Descripción de la resolución aplicada.</param>
        /// <param name="estado">Estado final del reclamo (ej. Completado, Rechazado).</param>
        public void DispararNotificacionResolucion(string traceId, long idReclamo, string descripcionResolucion, string estado)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                _ = Task.Run(async () =>
                {
                    using var innerScope = this._serviceProvider.CreateScope();
                    var repo = innerScope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                    var reclamo = await repo.ConsultarAsync<ReclamoEntity>(traceId, r => r.Id == idReclamo);
                    if (reclamo is not null)
                        await this._notificarResolucionUsuarioService.NotificarResolucionAsync(traceId, idReclamo, reclamo.IdUsuarioExterno, descripcionResolucion, estado);
                });
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
