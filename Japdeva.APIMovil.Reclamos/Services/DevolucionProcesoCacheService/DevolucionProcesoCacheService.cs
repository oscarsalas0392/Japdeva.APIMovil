using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.DevolucionProcesoCacheService
{
    public class DevolucionProcesoCacheService : IDevolucionProcesoCacheService
    {
        private readonly ILogger<DevolucionProcesoCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<DevolucionProcesoEntity> _devolucionProcesoCache = new List<DevolucionProcesoEntity>();
        private const int PAGINA_INICIAL = 1;
        private const bool ESTADO_ACTIVO = true;

        public DevolucionProcesoCacheService(IServiceProvider serviceProvider, ILogger<DevolucionProcesoCacheService> logger)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        public async Task LlenarCacheDevolucionProcesoAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                var devolucionProcesos = await consultarRepository.ConsultarListaAsync<DevolucionProcesoEntity>(
                    traceId, PAGINA_INICIAL, p => p.Activo == ESTADO_ACTIVO);
                if (devolucionProcesos is null || !devolucionProcesos.Lista.Any()) return;
                lock (this._devolucionProcesoCache)
                {
                    this._devolucionProcesoCache.Clear();
                    this._devolucionProcesoCache.AddRange(devolucionProcesos.Lista);
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

        public DevolucionProcesoEntity? ObtenerDevolucionProceso(string traceId, int idDevolucionProceso)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                lock (this._devolucionProcesoCache)
                {
                    return this._devolucionProcesoCache.FirstOrDefault(p => p.Id == idDevolucionProceso);
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
