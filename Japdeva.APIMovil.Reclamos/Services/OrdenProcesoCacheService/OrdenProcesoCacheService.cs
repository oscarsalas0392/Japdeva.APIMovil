using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.OrdenProcesoCacheService
{
    public class OrdenProcesoCacheService
    {
        private readonly ILogger<OrdenProcesoCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<OrdenProcesoEntity> _ordenProcesosCache = new List<OrdenProcesoEntity>();
        private const int PAGINA_INICIAL = 1;
        private const bool ESTADO_ACTIVO = true;
        public OrdenProcesoCacheService(IServiceProvider serviceProvider, ILogger<OrdenProcesoCacheService> logger)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }
        public async Task LlenarCacheOrdenProcesoAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                var ordenesProceso = await consultarRepository.ConsultarListaAsync<OrdenProcesoEntity>(
                    traceId, PAGINA_INICIAL, p => p.Activo == ESTADO_ACTIVO);
                if (ordenesProceso is null || !ordenesProceso.Lista.Any()) return;
                lock (this._ordenProcesosCache)
                {
                    this._ordenProcesosCache.Clear();
                    this._ordenProcesosCache.AddRange(ordenesProceso.Lista);
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
        public OrdenProcesoEntity? ObtenerOrdenProceso(string traceId, int idOrdenProceso)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                lock (this._ordenProcesosCache)
                {
                    return this._ordenProcesosCache.FirstOrDefault(p => p.Id == idOrdenProceso);
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
