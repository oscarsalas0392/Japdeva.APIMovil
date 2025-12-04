using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Reclamos.Services.DevolucionProcesoCacheService;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoCacheService;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoOrdenProcesoCacheService;
using Japdeva.APIMovil.Reclamos.Services.EstadoReclamoCacheService;
using Japdeva.APIMovil.Reclamos.Services.OrdenProcesoCacheService;

namespace Japdeva.APIMovil.Reclamos.BackgroundServices
{
    public class ParametrosBackGroundService : BackgroundService
    {
        private readonly ILogger<ParametrosBackGroundService> _logger;
        private readonly IDevolucionProcesoCacheService _devolucionProcesoCacheService;
        private readonly IEstadoDetalleReclamoCacheService _estadoDetalleReclamoCacheService;
        private readonly IEstadoDetalleReclamoOrdenProcesoCacheService _estadoDetalleReclamoOrdenProcesoCacheService;
        private readonly IEstadoReclamoCacheService _estadoReclamoCacheService;
        private readonly IOrdenProcesoCacheService _ordenProcesoCacheService;
        private const int DELAY_MINUTES = 10;
        private const string TRACE_ID = "N/A";

        public ParametrosBackGroundService(
            ILogger<ParametrosBackGroundService> logger,
            IDevolucionProcesoCacheService devolucionProcesoCacheService,
            IEstadoDetalleReclamoCacheService estadoDetalleReclamoCacheService,
            IEstadoDetalleReclamoOrdenProcesoCacheService estadoDetalleReclamoOrdenProcesoCacheService,
            IEstadoReclamoCacheService estadoReclamoCacheService,
            IOrdenProcesoCacheService ordenProcesoCacheService)
        {
            this._logger = logger;
            this._devolucionProcesoCacheService = devolucionProcesoCacheService;
            this._estadoDetalleReclamoCacheService = estadoDetalleReclamoCacheService;
            this._estadoDetalleReclamoOrdenProcesoCacheService = estadoDetalleReclamoOrdenProcesoCacheService;
            this._estadoReclamoCacheService = estadoReclamoCacheService;
            this._ordenProcesoCacheService = ordenProcesoCacheService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(TRACE_ID, nombreMetodo);
                while (!stoppingToken.IsCancellationRequested)
                {
                    await this._devolucionProcesoCacheService.LlenarCacheDevolucionProcesoAsync(TRACE_ID);
                    await this._estadoDetalleReclamoCacheService.LlenarCacheEstadoDetalleReclamoAsync(TRACE_ID);
                    await this._estadoDetalleReclamoOrdenProcesoCacheService.LlenarCacheEstadoDetalleReclamoOrdenProcesoAsync(TRACE_ID);
                    await this._estadoReclamoCacheService.LlenarCacheEstadoReclamoAsync(TRACE_ID);
                    await this._ordenProcesoCacheService.LlenarCacheOrdenProcesoAsync(TRACE_ID);
                    await Task.Delay(TimeSpan.FromMinutes(DELAY_MINUTES), stoppingToken);
                }
            }
            catch (Exception ex)
            {
                this._logger.Error(TRACE_ID, nombreMetodo, ex);
            }
            finally
            {
                this._logger.Fin(TRACE_ID, nombreMetodo);
            }
        }
    }
}
