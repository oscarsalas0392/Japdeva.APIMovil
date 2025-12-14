using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.NivelProcesoCacheService
{
    /// <summary>
    /// Servicio de cache para la gestión de órdenes de proceso en memoria.
    /// Proporciona acceso rápido a la configuración del workflow de reclamos,
    /// almacenando en cache las etapas y secuencias del proceso de atención para optimizar el rendimiento.
    /// </summary>
    public class NivelProcesoCacheService : INivelProcesoCacheService
    {
        private readonly ILogger<NivelProcesoCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<NivelProcesoEntity> _nivelProcesoEntityCache = new List<NivelProcesoEntity>();
        private const int PAGINA_INICIAL = 1;
        private const int NIVEL_INICIAL = 1;
        private const bool ESTADO_ACTIVO = true;

        /// <summary>
        /// Inicializa una nueva instancia del servicio de cache de órdenes de proceso.
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias y gestión de scopes.</param>
        /// <param name="logger">Logger para registro de eventos y errores del servicio de cache.</param>
        public NivelProcesoCacheService(IServiceProvider serviceProvider, ILogger<NivelProcesoCacheService> logger)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Carga el cache con todas las órdenes de proceso activas desde la base de datos.
        /// Actualiza completamente el cache con la configuración más reciente del workflow de reclamos.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        public async Task LlenarCacheNivelProcesoAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                var ordenProcesos = await consultarRepository.ConsultarListaAsync<NivelProcesoEntity>(
                    traceId, PAGINA_INICIAL, p => p.Activo == ESTADO_ACTIVO);
                if (ordenProcesos is null || !ordenProcesos.Lista.Any()) return;
                lock (this._nivelProcesoEntityCache)
                {
                    this._nivelProcesoEntityCache.Clear();
                    this._nivelProcesoEntityCache.AddRange(ordenProcesos.Lista);
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
        /// Obtiene un nivel de proceso específico del cache por su identificador.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idNivelProceso">Identificador del nivel de proceso a buscar.</param>
        /// <returns>
        /// La entidad <see cref="NivelProcesoEntity"/> correspondiente al identificador proporcionado,
        /// o <c>null</c> si no se encuentra un nivel de proceso activo con ese identificador.
        /// </returns>
        public NivelProcesoEntity? ObtenerNivelProcesoPorId(string traceId, int idNivelProceso)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                lock (this._nivelProcesoEntityCache)
                {
                    return this._nivelProcesoEntityCache.FirstOrDefault(p => p.Id == idNivelProceso && p.Activo == ESTADO_ACTIVO);
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
        /// Obtiene el primer nivel de proceso activo del cache.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <returns>
        /// La entidad <see cref="NivelProcesoEntity"/> correspondiente al primer nivel activo,
        /// o <c>null</c> si no se encuentra un nivel de proceso activo con el nivel inicial.
        /// </returns>
        public NivelProcesoEntity? ObtenerPrimerNivel(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                lock (this._nivelProcesoEntityCache)
                {
                    return this._nivelProcesoEntityCache.FirstOrDefault(p=> p.Nivel == NIVEL_INICIAL && p.Activo == ESTADO_ACTIVO);
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
