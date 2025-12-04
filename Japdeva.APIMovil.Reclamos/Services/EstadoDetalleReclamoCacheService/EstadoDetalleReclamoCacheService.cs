using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoCacheService
{
    public class EstadoDetalleReclamoCacheService : IEstadoDetalleReclamoCacheService
    {
        private readonly ILogger<EstadoDetalleReclamoCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<EstadoDetalleReclamoEntity> _estadoDetalleReclamoEntityCache = new List<EstadoDetalleReclamoEntity>();
        private const int PAGINA_INICIAL = 1;
        private const bool ESTADO_ACTIVO = true;
        public EstadoDetalleReclamoCacheService(IServiceProvider serviceProvider, ILogger<EstadoDetalleReclamoCacheService> logger)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        public async Task LlenarCacheEstadoDetalleReclamoAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                var estadosDetalleReclamo = await consultarRepository.ConsultarListaAsync<EstadoDetalleReclamoEntity>(
                    traceId, PAGINA_INICIAL, p => p.Activo == ESTADO_ACTIVO);
                if (estadosDetalleReclamo is null || !estadosDetalleReclamo.Lista.Any()) return;
                lock (this._estadoDetalleReclamoEntityCache)
                {
                    this._estadoDetalleReclamoEntityCache.Clear();
                    this._estadoDetalleReclamoEntityCache.AddRange(estadosDetalleReclamo.Lista);
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

        public EstadoDetalleReclamoEntity? ObtenerEstadoDetalleReclamo(string traceId, int idEstadoDetalleReclamo)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                lock (this._estadoDetalleReclamoEntityCache)
                {
                    return this._estadoDetalleReclamoEntityCache.FirstOrDefault(p => p.Id == idEstadoDetalleReclamo);
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
