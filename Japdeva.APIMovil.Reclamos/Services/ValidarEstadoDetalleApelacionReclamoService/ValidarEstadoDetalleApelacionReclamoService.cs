using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.AgregarApelacionReclamoDetalleService;
using Japdeva.APIMovil.Reclamos.Services.EditarApelacionReclamoService;
using Japdeva.APIMovil.Reclamos.Services.EditarDepartamentoApelacionReclamoService;
using Japdeva.APIMovil.Reclamos.Services.NotificarDepartamentoService;
using Japdeva.APIMovil.Reclamos.Services.NotificarResolucionUsuarioService;
using Japdeva.APIMovil.Reclamos.Services.OrdenNivelProcesoCacheService;

namespace Japdeva.APIMovil.Reclamos.Services.ValidarEstadoDetalleApelacionReclamoService
{
    /// <summary>
    /// Motor de workflow para el detalle de apelación de reclamo.
    /// Evalúa el estado del detalle y ejecuta la acción correspondiente:
    /// rechazar, finalizar, devolver al nivel anterior o avanzar al siguiente nivel.
    /// </summary>
    public class ValidarEstadoDetalleApelacionReclamoService : IValidarEstadoDetalleApelacionReclamoService
    {
        private readonly ILogger<ValidarEstadoDetalleApelacionReclamoService> _logger;
        private readonly IAgregarApelacionReclamoDetalleService _agregarApelacionReclamoDetalleService;
        private readonly IOrdenNivelProcesoCacheService _ordenNivelProcesoCacheService;
        private readonly IEditarApelacionReclamoService _editarApelacionReclamoService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEditarDepartamentoApelacionReclamoService _editarDepartamentoApelacionReclamoService;
        private readonly INotificarDepartamentoService _notificarDepartamentoService;
        private readonly INotificarResolucionUsuarioService _notificarResolucionUsuarioService;

        private const string ESTADO_RECHAZADO = nameof(EstadoReclamoModel.Rechazado);
        private const string ESTADO_COMPLETADO = nameof(EstadoReclamoModel.Completado);
        private const string MENSAJE_ERROR_ORDEN_NIVEL_PROCESO_NO_ENCONTRADO = "No se encontró la configuración de orden de nivel de proceso para IdNivelSuperior: {0} e IdNivelInferior: {1}";
        private const string MENSAJE_ERROR_ORDEN_NIVEL_PROCESO_NO_ES_DEVOLUCION = "No se encontró la configuración de devolución del orden de nivel de proceso para IdNivelSuperior: {0} e IdNivelInferior: {1}";
        private const string MENSAJE_ERROR_DESCRIPCION_RESOLUCION_OBLIGATORIA = "La descripción de la resolución es obligatoria";
        private const string MENSAJE_ERROR_NIVEL_PROCESO_NO_ENCONTRADO = "El nivel proceso con Id {0} no fue encontrado.";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ValidarEstadoDetalleApelacionReclamoService"/>.
        /// </summary>
        public ValidarEstadoDetalleApelacionReclamoService(
            ILogger<ValidarEstadoDetalleApelacionReclamoService> logger,
            IAgregarApelacionReclamoDetalleService agregarApelacionReclamoDetalleService,
            IOrdenNivelProcesoCacheService ordenNivelProcesoCacheService,
            IEditarApelacionReclamoService editarApelacionReclamoService,
            IServiceProvider serviceProvider,
            IEditarDepartamentoApelacionReclamoService editarDepartamentoApelacionReclamoService,
            INotificarDepartamentoService notificarDepartamentoService,
            INotificarResolucionUsuarioService notificarResolucionUsuarioService)
        {
            this._logger = logger;
            this._agregarApelacionReclamoDetalleService = agregarApelacionReclamoDetalleService;
            this._ordenNivelProcesoCacheService = ordenNivelProcesoCacheService;
            this._editarApelacionReclamoService = editarApelacionReclamoService;
            this._serviceProvider = serviceProvider;
            this._editarDepartamentoApelacionReclamoService = editarDepartamentoApelacionReclamoService;
            this._notificarDepartamentoService = notificarDepartamentoService;
            this._notificarResolucionUsuarioService = notificarResolucionUsuarioService;
        }

        /// <summary>
        /// Valida el estado del detalle de una apelación y ejecuta la acción correspondiente según las flags del estado.
        /// </summary>
        public async Task ValidarEstadoDetalleApelacionReclamoAsync(string traceId, EstadoDetalleReclamoEntity estadoDetalle, long idApelacionReclamo, int idNivelActual, int idNivelSiguienteProceso, string descripcionResolucion)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                if (estadoDetalle.RechazaProceso)
                {
                    if (string.IsNullOrWhiteSpace(descripcionResolucion)) throw new ArgumentException(MENSAJE_ERROR_DESCRIPCION_RESOLUCION_OBLIGATORIA);
                    await this._editarApelacionReclamoService.EditarApelacionReclamoAsync(traceId, idApelacionReclamo, descripcionResolucion, (int)EstadoReclamoModel.Rechazado);
                    this.DispararNotificacionResolucion(traceId, idApelacionReclamo, descripcionResolucion, ESTADO_RECHAZADO);
                    return;
                }

                if (estadoDetalle.FinalizarProceso)
                {
                    if (string.IsNullOrWhiteSpace(descripcionResolucion)) throw new ArgumentException(MENSAJE_ERROR_DESCRIPCION_RESOLUCION_OBLIGATORIA);
                    await this._editarApelacionReclamoService.EditarApelacionReclamoAsync(traceId, idApelacionReclamo, descripcionResolucion, (int)EstadoReclamoModel.Completado);
                    this.DispararNotificacionResolucion(traceId, idApelacionReclamo, descripcionResolucion, ESTADO_COMPLETADO);
                    return;
                }

                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();

                var nivelProceso = await consultarRepository.ConsultarAsync<NivelProcesoEntity>(traceId, x => x.Id == idNivelSiguienteProceso && x.IdProceso == (int)ProcesoModel.Apelacion);
                if (nivelProceso is null) throw new Exception(string.Format(MENSAJE_ERROR_NIVEL_PROCESO_NO_ENCONTRADO, idNivelSiguienteProceso));

                if (estadoDetalle.DevolucionProceso)
                {
                    var ordenNivelProceso = this._ordenNivelProcesoCacheService.ObtenerOrdenNivelProcesoCache(traceId, idNivelActual, nivelProceso.Id);
                    if (ordenNivelProceso is null) throw new Exception(string.Format(MENSAJE_ERROR_ORDEN_NIVEL_PROCESO_NO_ENCONTRADO, idNivelActual, nivelProceso.Id));
                    if (!ordenNivelProceso.DevolucionNivel) throw new Exception(string.Format(MENSAJE_ERROR_ORDEN_NIVEL_PROCESO_NO_ES_DEVOLUCION, idNivelActual, nivelProceso.Id));
                }

                Task tareaAgregarDetalle = this._agregarApelacionReclamoDetalleService.AgregarApelacionReclamoDetalleAsync(traceId, idApelacionReclamo, idNivelSiguienteProceso);
                Task tareaActualizarDepartamento = this._editarDepartamentoApelacionReclamoService.EditarDepartamentoApelacionReclamoAsync(traceId, idApelacionReclamo, nivelProceso.IdDepartamento);
                await Task.WhenAll(tareaAgregarDetalle, tareaActualizarDepartamento);

                long idDepartamento = nivelProceso.IdDepartamento;
                _ = Task.Run(async () =>
                {
                    var apelacion = await consultarRepository.ConsultarAsync<ApelacionReclamoEntity>(traceId, a => a.Id == idApelacionReclamo);
                    if (apelacion is not null)
                        await this._notificarDepartamentoService.NotificarNuevoReclamoAsync(traceId, idDepartamento, idApelacionReclamo, apelacion.IdUsuarioExterno);
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
        /// Dispara en segundo plano la notificación de resolución al usuario propietario de la apelación.
        /// </summary>
        public void DispararNotificacionResolucion(string traceId, long idApelacionReclamo, string descripcionResolucion, string estado)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                _ = Task.Run(async () =>
                {
                    using var innerScope = this._serviceProvider.CreateScope();
                    var repo = innerScope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                    var apelacion = await repo.ConsultarAsync<ApelacionReclamoEntity>(traceId, a => a.Id == idApelacionReclamo);
                    if (apelacion is not null)
                        await this._notificarResolucionUsuarioService.NotificarResolucionAsync(traceId, idApelacionReclamo, apelacion.IdUsuarioExterno, descripcionResolucion, estado);
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
