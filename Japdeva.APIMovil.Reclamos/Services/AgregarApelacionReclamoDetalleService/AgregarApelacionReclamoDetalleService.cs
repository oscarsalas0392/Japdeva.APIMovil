using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoCacheService;
using Japdeva.APIMovil.Reclamos.Services.NivelProcesoCacheService;

namespace Japdeva.APIMovil.Reclamos.Services.AgregarApelacionReclamoDetalleService
{
    /// <summary>
    /// Servicio para agregar detalles de seguimiento a una apelación de reclamo.
    /// </summary>
    public class AgregarApelacionReclamoDetalleService : IAgregarApelacionReclamoDetalleService
    {
        private readonly ILogger<AgregarApelacionReclamoDetalleService> _logger;
        private readonly INivelProcesoCacheService _nivelProcesoCacheService;
        private readonly IEstadoDetalleReclamoCacheService _estadoDetalleReclamoCacheService;
        private readonly IServiceProvider _serviceProvider;

        private const string MENSAJE_ERROR_NIVEL_PROCESO_NO_ENCONTRADO = "El nivel del proceso con Id {0} no fue encontrado.";
        private const string MENSAJE_ERROR_ESTADO_DETALLE_PROCESO_NO_ENCONTRADO = "El estado detalle proceso con Id {0} no fue encontrado.";
        private const int VALOR_DEFECTO_ID_USUARIO_INTERNO = 0;
        private const string VALOR_DEFECTO_DESCRIPCION = "";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AgregarApelacionReclamoDetalleService"/>.
        /// </summary>
        public AgregarApelacionReclamoDetalleService(
            ILogger<AgregarApelacionReclamoDetalleService> logger,
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
        /// Agrega un nuevo detalle a una apelación existente.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idApelacionReclamo">Identificador de la apelación.</param>
        /// <param name="idNivelProceso">Identificador del nivel de proceso asociado.</param>
        public async Task AgregarApelacionReclamoDetalleAsync(string traceId, long idApelacionReclamo, int idNivelProceso)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var serviceScope = this._serviceProvider.CreateScope();
                var agregarRepository = serviceScope.ServiceProvider.GetRequiredService<IAgregarRepository>();

                var nivelProceso = this._nivelProcesoCacheService.ObtenerNivelProcesoPorId(traceId, idNivelProceso);
                if (nivelProceso is null || nivelProceso.IdProceso != (int)ProcesoModel.Apelacion) throw new Exception(string.Format(MENSAJE_ERROR_NIVEL_PROCESO_NO_ENCONTRADO, idNivelProceso));

                var estadoDetalle = this._estadoDetalleReclamoCacheService.ObtenerEstadoDetalleReclamo(traceId, (int)EstadoDetalleReclamoModel.Pendiente);
                if (estadoDetalle is null) throw new Exception(string.Format(MENSAJE_ERROR_ESTADO_DETALLE_PROCESO_NO_ENCONTRADO, (int)EstadoDetalleReclamoModel.Pendiente));

                DetalleApelacionReclamoEntity detalle = new DetalleApelacionReclamoEntity();
                detalle.IdApelacionReclamo = idApelacionReclamo;
                detalle.IdNivelProceso = nivelProceso.Id;
                detalle.Descripcion = VALOR_DEFECTO_DESCRIPCION;
                detalle.FechaRegistro = DateTime.UtcNow;
                detalle.IdUsuarioInterno = VALOR_DEFECTO_ID_USUARIO_INTERNO;
                detalle.IdEstadoDetalleReclamo = estadoDetalle.Id;
                detalle.IdDepartamento = nivelProceso.IdDepartamento;

                await agregarRepository.AgregarAsync<DetalleApelacionReclamoEntity>(traceId, detalle);
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
