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
        private readonly IAgregarRepository _agregarRepository;
        private readonly IConsultarRepository _consultarRepository;
        private readonly INivelProcesoCacheService _nivelProcesoCacheService;
        private readonly IEstadoDetalleReclamoCacheService _estadoDetalleReclamoCacheService;

        private const string MENSAJE_ERROR_RECLAMO_NO_ENCONTRADO = "El reclamo con Id {0} no fue encontrado.";
        private const string MENSAJE_ERROR_NIVEL_PROCESO_NO_ENCONTRADO = "El nivel del proceso con Id {0} no fue encontrado.";
        private const string MENSAJE_ERROR_ESTADO_DETALLE_PROCESO_NO_ENCONTRADO = "El estado detalle proceso con Id {0} no fue encontrado.";

        private const int VALOR_DEFECTO_ID_USUARIO_INTERNO = 0;
        private const string VALOR_DEFECTO_DESCRIPCION = "";

        /// <summary>
        /// Inicializa una nueva instancia del servicio de agregación de detalles de reclamo.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos y errores.</param>
        /// <param name="agregarRepository">Repositorio para operaciones de inserción en base de datos.</param>
        /// <param name="consultarRepository">Repositorio para operaciones de consulta en base de datos.</param>
        public AgregarReclamoDetalleService(ILogger<AgregarReclamoDetalleService> logger,
            IAgregarRepository agregarRepository,
            IConsultarRepository consultarRepository,
            INivelProcesoCacheService nivelProcesoCacheService,
            IEstadoDetalleReclamoCacheService estadoDetalleReclamoCacheService)
        {
            this._logger = logger;
            this._agregarRepository = agregarRepository;
            this._consultarRepository = consultarRepository;
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

                var reclamo = await this._consultarRepository.ConsultarAsync<ReclamoEntity>(traceId, reclamo => reclamo.Id == idReclamo);
                if (reclamo is null) throw new Exception(string.Format(MENSAJE_ERROR_RECLAMO_NO_ENCONTRADO, idReclamo));

                var nivelProceso = this._nivelProcesoCacheService.ObtenerNivelProcesoPorId(traceId, idNivelSiguienteProceso);
                if(nivelProceso is null) throw new Exception(string.Format(MENSAJE_ERROR_NIVEL_PROCESO_NO_ENCONTRADO, idNivelSiguienteProceso));

                var estadoDetalle = this._estadoDetalleReclamoCacheService.ObtenerEstadoDetalleReclamo(traceId, (int)EstadoDetalleReclamoModel.Pendiente);
                if (estadoDetalle is null) throw new Exception(string.Format(MENSAJE_ERROR_ESTADO_DETALLE_PROCESO_NO_ENCONTRADO, (int)EstadoDetalleReclamoModel.Pendiente));

                DetalleReclamoEntity reclamoDetalle = new DetalleReclamoEntity();
                reclamoDetalle.IdReclamo = reclamo.Id;
                reclamoDetalle.IdNivelProceso = nivelProceso.Id;
                reclamoDetalle.Descripcion = VALOR_DEFECTO_DESCRIPCION;
                reclamoDetalle.FechaRegistro = DateTime.Now;
                reclamoDetalle.IdUsuarioInterno = VALOR_DEFECTO_ID_USUARIO_INTERNO;
                reclamoDetalle.IdEstadoDetalleReclamo = estadoDetalle.Id;
                reclamoDetalle.IdDepartamento = nivelProceso.IdDepartamento;

                await this._agregarRepository.AgregarAsync<DetalleReclamoEntity>(traceId, reclamoDetalle);
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
