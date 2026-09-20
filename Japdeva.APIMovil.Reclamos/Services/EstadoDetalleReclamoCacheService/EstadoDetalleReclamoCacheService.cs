using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;


namespace Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoCacheService
{
    /// <summary>
    /// Servicio de cache para la gestión de estados de detalle de reclamos en memoria.
    /// Proporciona acceso rápido y eficiente a los catálogos de estados de detalles,
    /// mejorando el rendimiento del sistema al evitar consultas repetitivas a la base de datos.
    /// </summary>
    public class EstadoDetalleReclamoCacheService : IEstadoDetalleReclamoCacheService
    {
        private readonly ILogger<EstadoDetalleReclamoCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<EstadoDetalleReclamoEntity> _estadoDetalleReclamoEntityCache = new List<EstadoDetalleReclamoEntity>();
        private const int PAGINA_INICIAL = 1;
        private const bool ESTADO_ACTIVO = true;


        /// <summary>
        /// Inicializa una nueva instancia del servicio de cache de estados de detalle de reclamo.
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias.</param>
        /// <param name="logger">Logger para registro de eventos del servicio de cache.</param>
        public EstadoDetalleReclamoCacheService(IServiceProvider serviceProvider, ILogger<EstadoDetalleReclamoCacheService> logger)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Carga el cache con todos los estados de detalle de reclamo activos desde la base de datos.
        /// Reemplaza completamente el contenido del cache con los datos más recientes.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
           public async Task LlenarCacheEstadoDetalleReclamoAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                var estadosDetalleReclamo = await consultarListaRepository.ConsultarListaAsync<EstadoDetalleReclamoEntity>(
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

        /// <summary>
        /// Obtiene un estado de detalle de reclamo específico desde el cache en memoria.
        /// Realiza una búsqueda eficiente en el cache por ID y estado activo.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idEstadoDetalleReclamo">Identificador del estado de detalle de reclamo a buscar.</param>
        /// <returns>La entidad del estado de detalle de reclamo si se encuentra y está activa, null en caso contrario.</returns>
        public EstadoDetalleReclamoEntity? ObtenerEstadoDetalleReclamo(string traceId, int idEstadoDetalleReclamo)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                return this._estadoDetalleReclamoEntityCache.FirstOrDefault(p => p.Id == idEstadoDetalleReclamo && p.Activo == ESTADO_ACTIVO);
                
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
