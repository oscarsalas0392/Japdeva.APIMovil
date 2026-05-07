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
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="estadoDetalle">Entidad que representa el estado del detalle.</param>
        /// <param name="idApelacionReclamo">Identificador de la apelación.</param>
        /// <param name="idNivelActual">Identificador del nivel actual del proceso.</param>
        /// <param name="idNivelSiguienteProceso">Identificador del siguiente nivel del proceso.</param>
        /// <param name="descripcionResolucion">Descripción de la resolución aplicada.</param>
        public async Task ValidarEstadoDetalleApelacionReclamoAsync(string traceId, EstadoDetalleReclamoEntity estadoDetalle, long idApelacionReclamo, int idNivelActual, int idNivelSiguienteProceso, string descripcionResolucion)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                if (await this.ManejarFinalizacionApelacionAsync(traceId, estadoDetalle, idApelacionReclamo, descripcionResolucion))
                    return;

                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();

                var nivelProceso = await consultarRepository.ConsultarAsync<NivelProcesoEntity>(traceId, x => x.Id == idNivelSiguienteProceso && x.IdProceso == (int)ProcesoModel.Apelacion);
                if (nivelProceso is null) throw new Exception(string.Format(MENSAJE_ERROR_NIVEL_PROCESO_NO_ENCONTRADO, idNivelSiguienteProceso));

                this.ValidarDevolucionNivelApelacion(traceId, estadoDetalle, idNivelActual, nivelProceso.Id);

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

        private const bool PROCESO_FINALIZADO = true;
        private const bool PROCESO_CONTINUA = false;

        /// <summary>
        /// Evalúa si el estado finaliza la apelación por rechazo o completado, ejecuta la acción correspondiente y notifica al usuario.
        /// Retorna verdadero si el proceso fue finalizado, falso si debe continuar al siguiente nivel.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="estadoDetalle">Entidad del estado del detalle a evaluar.</param>
        /// <param name="idApelacionReclamo">Identificador de la apelación.</param>
        /// <param name="descripcionResolucion">Descripción de la resolución aplicada.</param>
        /// <returns>Verdadero si el proceso fue finalizado, falso si debe avanzar.</returns>
        public async Task<bool> ManejarFinalizacionApelacionAsync(string traceId, EstadoDetalleReclamoEntity estadoDetalle, long idApelacionReclamo, string descripcionResolucion)
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
                    return PROCESO_FINALIZADO;
                }

                if (estadoDetalle.FinalizarProceso)
                {
                    if (string.IsNullOrWhiteSpace(descripcionResolucion)) throw new ArgumentException(MENSAJE_ERROR_DESCRIPCION_RESOLUCION_OBLIGATORIA);
                    await this._editarApelacionReclamoService.EditarApelacionReclamoAsync(traceId, idApelacionReclamo, descripcionResolucion, (int)EstadoReclamoModel.Completado);
                    this.DispararNotificacionResolucion(traceId, idApelacionReclamo, descripcionResolucion, ESTADO_COMPLETADO);
                    return PROCESO_FINALIZADO;
                }

                return PROCESO_CONTINUA;
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
        /// Valida que la devolución al nivel anterior sea válida según la configuración de orden de niveles de apelación.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="estadoDetalle">Entidad del estado del detalle a evaluar.</param>
        /// <param name="idNivelActual">Identificador del nivel actual del proceso.</param>
        /// <param name="idNivelSiguiente">Identificador del nivel al que se devuelve.</param>
        public void ValidarDevolucionNivelApelacion(string traceId, EstadoDetalleReclamoEntity estadoDetalle, int idNivelActual, int idNivelSiguiente)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (!estadoDetalle.DevolucionProceso) return;
                var ordenNivelProceso = this._ordenNivelProcesoCacheService.ObtenerOrdenNivelProcesoCache(traceId, idNivelActual, idNivelSiguiente);
                if (ordenNivelProceso is null) throw new Exception(string.Format(MENSAJE_ERROR_ORDEN_NIVEL_PROCESO_NO_ENCONTRADO, idNivelActual, idNivelSiguiente));
                if (!ordenNivelProceso.DevolucionNivel) throw new Exception(string.Format(MENSAJE_ERROR_ORDEN_NIVEL_PROCESO_NO_ES_DEVOLUCION, idNivelActual, idNivelSiguiente));
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
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idApelacionReclamo">Identificador de la apelación resuelta.</param>
        /// <param name="descripcionResolucion">Descripción de la resolución aplicada.</param>
        /// <param name="estado">Estado final de la apelación.</param>
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
