using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.EstadoReclamoCacheService
{
    /// <summary>
    /// Servicio de cache para la gestión de estados de reclamos en memoria.
    /// Proporciona acceso rápido y eficiente a los catálogos de estados de reclamos principales,
    /// optimizando las consultas frecuentes al evitar accesos repetitivos a la base de datos.
    /// </summary>
    public class EstadoReclamoCacheService : IEstadoReclamoCacheService
    {
        private readonly ILogger<EstadoReclamoCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<EstadoReclamoEntity> _estadoReclamoEntityCache = new List<EstadoReclamoEntity>();
        private const int PAGINA_INICIAL = 1;
        private const bool ESTADO_ACTIVO = true;

        /// <summary>
        /// Inicializa una nueva instancia del servicio de cache de estados de reclamo.
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias y creación de scopes.</param>
        /// <param name="logger">Logger para registro de eventos y errores del servicio de cache.</param>
        public EstadoReclamoCacheService(IServiceProvider serviceProvider, ILogger<EstadoReclamoCacheService> logger)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Carga el cache con todos los estados de reclamo activos desde la base de datos.
        /// Actualiza completamente el contenido del cache con los datos más recientes disponibles.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
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
                lock (this._estadoReclamoEntityCache)
                {
                    this._estadoReclamoEntityCache.Clear();
                    this._estadoReclamoEntityCache.AddRange(estadosReclamo.Lista);
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
        /// Obtiene un estado de reclamo específico desde el cache en memoria.
        /// Realiza una búsqueda thread-safe en el cache por ID y estado activo.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idEstadoReclamo">Identificador del estado de reclamo a buscar.</param>
        /// <returns>La entidad del estado de reclamo si se encuentra y está activa, null en caso contrario.</returns>
        public EstadoReclamoEntity? ObtenerEstadoReclamo(string traceId, int idEstadoReclamo)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                lock (this._estadoReclamoEntityCache)
                {
                    return this._estadoReclamoEntityCache.FirstOrDefault(p => p.Id == idEstadoReclamo && p.Activo == ESTADO_ACTIVO);
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
