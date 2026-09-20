using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;

namespace Japdeva.APIMovil.Colas.Services.PrioridadService
{

    /// <summary>
    /// Servicio para gestionar prioridades de mensajes en colas.
    /// </summary>
    public class PrioridadService : IPrioridadService
    {
        private readonly ILogger<PrioridadService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<PrioridadEntity> _prioridadesCache = new List<PrioridadEntity>();
        private const int PAGINA_INICIAL = 1;
        private const bool ESTADO_ACTIVO = true;

        /// <summary>
        /// Inicializa una nueva instancia de la clase PrioridadService.
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        /// <param name="logger">Instancia de logger para registrar eventos.</param>
        public PrioridadService(IServiceProvider serviceProvider, ILogger<PrioridadService> logger)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Llena el caché de prioridades con datos activos desde el repositorio.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        public async Task LlenarCachePrioridadesAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {

                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                var prioridades = await consultarRepository.ConsultarListaAsync<PrioridadEntity>(
                    traceId, PAGINA_INICIAL, p => p.Activo == ESTADO_ACTIVO);

                if (prioridades is null || !prioridades.Lista.Any()) return;

                lock (this._prioridadesCache)
                {
                    this._prioridadesCache.Clear();
                    this._prioridadesCache.AddRange(prioridades.Lista);
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

        /// <summary>
        /// Obtiene una prioridad específica por su identificador desde el caché.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <param name="id">Identificador único de la prioridad a buscar.</param>
        /// <returns>La entidad de prioridad encontrada o null si no existe.</returns>
        public PrioridadEntity? ObtenerPrioridadPorId(string traceId, int id)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                lock (this._prioridadesCache)
                {
                    return this._prioridadesCache.FirstOrDefault(p => p.Id == id);
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