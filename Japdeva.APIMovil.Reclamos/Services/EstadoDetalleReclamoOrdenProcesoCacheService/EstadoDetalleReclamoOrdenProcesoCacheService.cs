using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoOrdenProcesoCacheService
{
    public class EstadoDetalleReclamoOrdenProcesoCacheService : IEstadoDetalleReclamoOrdenProcesoCacheService
    {
        private readonly ILogger<EstadoDetalleReclamoOrdenProcesoCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<EstadoDetalleReclamoOrdenProcesoEntity> _estadoDetalleReclamoOrdenProcesoCache = new List<EstadoDetalleReclamoOrdenProcesoEntity>();
        private const int PAGINA_INICIAL = 1;
        private const bool ESTADO_ACTIVO = true;

        public EstadoDetalleReclamoOrdenProcesoCacheService(IServiceProvider serviceProvider, ILogger<EstadoDetalleReclamoOrdenProcesoCacheService> logger)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        public async Task LlenarCacheEstadoDetalleReclamoOrdenProcesoAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                var estadosDetalleReclamoOrdenProceso = await consultarRepository.ConsultarListaAsync<EstadoDetalleReclamoOrdenProcesoEntity>(
                    traceId, PAGINA_INICIAL, p => p.Activo == ESTADO_ACTIVO);
                if (estadosDetalleReclamoOrdenProceso is null || !estadosDetalleReclamoOrdenProceso.Lista.Any()) return;
                lock (this._estadoDetalleReclamoOrdenProcesoCache)
                {
                    this._estadoDetalleReclamoOrdenProcesoCache.Clear();
                    this._estadoDetalleReclamoOrdenProcesoCache.AddRange(estadosDetalleReclamoOrdenProceso.Lista);
                }
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

        public EstadoDetalleReclamoOrdenProcesoEntity? ObtenerEstadoDetalleReclamoOrdenProceso(string traceId, int idEstadoDetalleReclamoOrdenProceso)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                lock (this._estadoDetalleReclamoOrdenProcesoCache)
                {
                    return this._estadoDetalleReclamoOrdenProcesoCache.FirstOrDefault(p => p.Id == idEstadoDetalleReclamoOrdenProceso);
                }
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
