using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.EstadoReclamoCacheService
{
    public class EstadoReclamoCacheService : IEstadoReclamoCacheService
    {
        private readonly ILogger<EstadoReclamoCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<EstadoReclamoEntity> _estadoReclamosCache = new List<EstadoReclamoEntity>();
        private const int PAGINA_INICIAL = 1;
        private const bool ESTADO_ACTIVO = true;

        public EstadoReclamoCacheService(IServiceProvider serviceProvider, ILogger<EstadoReclamoCacheService> logger)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        public async Task LlenarCacheEstadoReclamoAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                var estadosReclamo = await consultarRepository.ConsultarListaAsync<EstadoReclamoEntity>(
                    traceId, PAGINA_INICIAL, p => p.Activo == ESTADO_ACTIVO);
                if (estadosReclamo is null || !estadosReclamo.Lista.Any()) return;
                lock (this._estadoReclamosCache)
                {
                    this._estadoReclamosCache.Clear();
                    this._estadoReclamosCache.AddRange(estadosReclamo.Lista);
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

        public EstadoReclamoEntity? ObtenerEstadoReclamo(string traceId, int idEstadoReclamo)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                lock (this._estadoReclamosCache)
                {
                    return this._estadoReclamosCache.FirstOrDefault(p => p.Id == idEstadoReclamo);
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
