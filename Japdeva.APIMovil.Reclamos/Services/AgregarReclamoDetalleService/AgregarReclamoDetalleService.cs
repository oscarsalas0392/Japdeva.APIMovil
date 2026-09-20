using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoCacheService;
using Japdeva.APIMovil.Reclamos.Services.NivelProcesoCacheService;


namespace Japdeva.APIMovil.Reclamos.Services.AgregarReclamoDetalleService
{
    /// <summary>
    /// Servicio para la gestión de detalles de reclamos en el sistema.
    /// Proporciona funcionalidades para agregar información detallada y seguimiento a reclamos existentes,
    /// incluyendo validaciones de integridad referencial.
    /// </summary>
    public class AgregarReclamoDetalleService : IAgregarReclamoDetalleService
    {

        private readonly ILogger<AgregarReclamoDetalleService> _logger;
        private readonly INivelProcesoCacheService _nivelProcesoCacheService;
        private readonly IEstadoDetalleReclamoCacheService _estadoDetalleReclamoCacheService;
        private readonly IServiceProvider _serviceProvider;


        private const string MENSAJE_ERROR_ESTADO_DETALLE_PROCESO_NO_ENCONTRADO = "El estado detalle proceso con Id {0} no fue encontrado.";
        private const string MENSAJE_ERROR_NIVEL_PROCESO_NO_ENCONTRADO = "El nivel del proceso con Id {0} no fue encontrado.";
        private const string MENSAJE_ERROR_RECLAMO_EN_HISTORICO = "No se pueden agregar detalles al reclamo con Id {0} porque está en histórico.";
        private const string MENSAJE_ERROR_RECLAMO_NO_ENCONTRADO = "El reclamo con Id {0} no fue encontrado.";

        private const int VALOR_DEFECTO_ID_USUARIO_INTERNO = 0;
        private const string VALOR_DEFECTO_DESCRIPCION = "";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AgregarReclamoDetalleService"/>.
        /// </summary>
        /// <param name="logger">Instancia del registrador de logs.</param>
        /// <param name="serviceProvider">Proveedor de servicios para la obtención de dependencias.</param>
        /// <param name="nivelProcesoCacheService">Servicio de caché para niveles de proceso.</param>
        /// <param name="estadoDetalleReclamoCacheService">Servicio de caché para estados de detalle de reclamo.</param>
        public AgregarReclamoDetalleService(ILogger<AgregarReclamoDetalleService> logger,
            IServiceProvider serviceProvider,
            INivelProcesoCacheService nivelProcesoCacheService,
            IEstadoDetalleReclamoCacheService estadoDetalleReclamoCacheService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._nivelProcesoCacheService = nivelProcesoCacheService;
            this._estadoDetalleReclamoCacheService = estadoDetalleReclamoCacheService;
        }

        /// <summary>
        /// Agrega un nuevo detalle a un reclamo existente.
        /// Valida la existencia del reclamo antes de crear el detalle y establece el estado inicial del detalle.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idReclamo">Identificador del reclamo al cual se agregará el detalle.</param>
        /// <param name="idNivelSiguienteProceso">Identificador del nivel de proceso asociado.</param>
        public async Task AgregarReclamoDetalleAsync(string traceId, long idReclamo, int idNivelSiguienteProceso)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var serviceScope = this._serviceProvider.CreateScope();
                var consultarRepository = serviceScope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var agregarRepository = serviceScope.ServiceProvider.GetRequiredService<IAgregarRepository>();

                var reclamo = await consultarRepository.ConsultarAsync<ReclamoEntity>(traceId, x => x.Id == idReclamo);
                if (reclamo is null) throw new KeyNotFoundException(string.Format(MENSAJE_ERROR_RECLAMO_NO_ENCONTRADO, idReclamo));
                if (reclamo.EstaEnHistorico) throw new InvalidOperationException(string.Format(MENSAJE_ERROR_RECLAMO_EN_HISTORICO, idReclamo));

                var nivelProceso = this._nivelProcesoCacheService.ObtenerNivelProcesoPorId(traceId, idNivelSiguienteProceso);
                if (nivelProceso is null || nivelProceso.IdProceso != (int)ProcesoModel.Reclamo) throw new Exception(string.Format(MENSAJE_ERROR_NIVEL_PROCESO_NO_ENCONTRADO, idNivelSiguienteProceso));

                var estadoDetalle = this._estadoDetalleReclamoCacheService.ObtenerEstadoDetalleReclamo(traceId, (int)EstadoDetalleReclamoModel.Pendiente);
                if (estadoDetalle is null) throw new Exception(string.Format(MENSAJE_ERROR_ESTADO_DETALLE_PROCESO_NO_ENCONTRADO, (int)EstadoDetalleReclamoModel.Pendiente));

                DetalleReclamoEntity reclamoDetalle = new DetalleReclamoEntity();
                reclamoDetalle.IdReclamo = idReclamo;
                reclamoDetalle.IdNivelProceso = nivelProceso.Id;
                reclamoDetalle.Descripcion = VALOR_DEFECTO_DESCRIPCION;
                reclamoDetalle.FechaRegistro = DateTime.UtcNow;
                reclamoDetalle.IdUsuarioInterno = VALOR_DEFECTO_ID_USUARIO_INTERNO;
                reclamoDetalle.IdEstadoDetalleReclamo = estadoDetalle.Id;
                reclamoDetalle.IdDepartamento = nivelProceso.IdDepartamento;
              

                await agregarRepository.AgregarAsync<DetalleReclamoEntity>(traceId, reclamoDetalle);
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
