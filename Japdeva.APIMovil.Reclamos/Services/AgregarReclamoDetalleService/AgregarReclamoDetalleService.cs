using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoCacheService;
using Japdeva.APIMovil.Reclamos.Services.OrdenProcesoCacheService;

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
        private readonly IOrdenProcesoCacheService _ordenProcesoCacheService;
        private readonly IEstadoDetalleReclamoCacheService _estadoDetalleReclamoCacheService;

        private const string MENSAJE_ERROR_RECLAMO_NO_ENCONTRADO = "El reclamo con Id {0} no fue encontrado.";
        private const string MENSAJE_ERROR_ORDEN_PROCESO_NO_ENCONTRADO = "El orden proceso con Id {0} no fue encontrado.";
        private const string MENSAJE_ERROR_ESTADO_DETALLE_PROCESO_NO_ENCONTRADO = "El estado detalle proceso con Id {0} no fue encontrado.";

        private const int VALOR_DEFECTO_ID_USUARIO_INTERNO = 0;
        private const int VALOR_DEFECTO_ID_ORDEN_PROCESO = 0;
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
            IOrdenProcesoCacheService ordenProcesoCacheService,
            IEstadoDetalleReclamoCacheService estadoDetalleReclamoCacheService)
        {
            this._logger = logger;
            this._agregarRepository = agregarRepository;
            this._consultarRepository = consultarRepository;
            this._ordenProcesoCacheService = ordenProcesoCacheService;
            this._estadoDetalleReclamoCacheService = estadoDetalleReclamoCacheService;
        }

        /// <summary>
        /// Agrega un nuevo detalle a un reclamo existente.
        /// Valida la existencia del reclamo antes de crear el detalle y establece el estado inicial del detalle.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idReclamo">Identificador del reclamo al cual se agregará el detalle.</param>
        /// <param name="descripcion">Descripción o comentario del detalle a agregar.</param>
        /// <param name="estadoDetalleReclamoModel">Estado inicial del detalle del reclamo. Valor por defecto: Pendiente.</param>
        /// <param name="idUsuarioInterno">Identificador del usuario interno que registra el detalle. Valor por defecto: 0.</param>
        /// <param name="idOrdenProceso">Identificador del orden de proceso asociado. Valor por defecto: 0.</param>
        public async Task AgregarReclamoDetalleAsync(
            string traceId,
            long idReclamo,
            string descripcion= VALOR_DEFECTO_DESCRIPCION,
            EstadoDetalleReclamoModel estadoDetalleReclamoModel = EstadoDetalleReclamoModel.Pendiente,
            int idUsuarioInterno = VALOR_DEFECTO_ID_USUARIO_INTERNO,
            int idOrdenProceso = VALOR_DEFECTO_ID_ORDEN_PROCESO)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                var reclamo = await this._consultarRepository.ConsultarAsync<ReclamoEntity>(traceId, reclamo => reclamo.Id == idReclamo);
                if (reclamo is null) throw new Exception(string.Format(MENSAJE_ERROR_RECLAMO_NO_ENCONTRADO, idReclamo));

                var ordenProceso = this._ordenProcesoCacheService.ObtenerOrdenProceso(traceId, idOrdenProceso);
                if(ordenProceso is null) throw new Exception(string.Format(MENSAJE_ERROR_ORDEN_PROCESO_NO_ENCONTRADO, idOrdenProceso));

                var estadoDetalle = this._estadoDetalleReclamoCacheService.ObtenerEstadoDetalleReclamo(traceId, (int)estadoDetalleReclamoModel);
                if (estadoDetalle is null) throw new Exception(string.Format(MENSAJE_ERROR_ESTADO_DETALLE_PROCESO_NO_ENCONTRADO, (int)estadoDetalleReclamoModel));

                DetalleReclamoEntity reclamoDetalle = new DetalleReclamoEntity();
                reclamoDetalle.IdReclamo = reclamo.Id;
                reclamoDetalle.IdOrdenProceso = ordenProceso.Id;
                reclamoDetalle.Descripcion = descripcion;
                reclamoDetalle.FechaRegistro = DateTime.Now;
                reclamoDetalle.IdUsuarioInterno = idUsuarioInterno;
                reclamoDetalle.IdEstadoDetalleReclamo = estadoDetalle.Id;

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
