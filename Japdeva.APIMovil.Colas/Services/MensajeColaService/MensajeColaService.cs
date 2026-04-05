using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Colas.Models;
using Japdeva.APIMovil.Colas.Repositories.MensajesColaRepository;
using Japdeva.APIMovil.Colas.Services.ColaService;
using Japdeva.APIMovil.Colas.Services.SuscripcionColaService;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;

namespace Japdeva.APIMovil.Colas.Services.MensajeColaService
{
    /// <summary>
    /// Servicio para gestionar mensajes en colas del sistema.
    /// </summary>
    public class MensajeColaService : IMensajeColaService
    {
        private readonly IColaService _colaService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MensajeColaService> _logger;
        private readonly ISuscripcionColaService _suscripcionColaService;
        private readonly List<MensajeColaEntity> _mensajesCache = new List<MensajeColaEntity>();
        private const string MENSAJE_ERROR_NOMBRE_COLA_INVALIDO = "El nombre de cola proporcionado no es válido.";
        private const bool TRACE_ID_DIFERENTE_INICIAL = false;
        private const int MAX_TAMANIO_CACHE = 500;

        /// <summary>
        /// Inicializa una nueva instancia de la clase MensajeColaService.
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias.</param>
        /// <param name="logger">Instancia de logger para registrar eventos.</param>
        /// <param name="colaService">Servicio para gestionar operaciones de colas.</param>
        /// <param name="suscripcionColaService">Servicio para notificar suscriptores de nuevos mensajes.</param>
        public MensajeColaService(
            IServiceProvider serviceProvider,
            ILogger<MensajeColaService> logger,
            IColaService colaService,
            ISuscripcionColaService suscripcionColaService)
        {
            this._logger = logger;
            this._colaService = colaService;
            this._serviceProvider = serviceProvider;
            this._suscripcionColaService = suscripcionColaService;
        }

        /// <summary>
        /// Busca un mensaje en el caché en memoria por su IdRpc y nombre de cola.
        /// No realiza ninguna consulta a la base de datos.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="nombreCola">Nombre de la cola donde buscar.</param>
        /// <param name="idRpc">Identificador RPC del mensaje.</param>
        /// <returns>El modelo del mensaje si está en caché, o null si no se encontró.</returns>
        public async Task<MensajeColasRespuestaModel?> BuscarEnCachePorIdRpcAsync(string traceId, string nombreCola, string idRpc)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                ColaEntity? cola = await this._colaService.ObtenerColaPorNombreAsync(traceId, nombreCola);
                if (cola is null) return null;
                MensajeColaEntity? entidad;
                lock (this._mensajesCache)
                {
                    entidad = this._mensajesCache.FirstOrDefault(m => m.ColaId == cola.Id && m.IdRpc == idRpc);
                }
                return entidad is null ? null : MapEntityToRespuestaModel(entidad);
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
        /// Llena el caché con mensajes pendientes y fallidos, notificando a los suscriptores de nuevos mensajes.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        public async Task LlenarCacheMensajesAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                using IServiceScope scope = this._serviceProvider.CreateScope();
                IMensajesColaRepository mensajesColaRepository = scope.ServiceProvider.GetRequiredService<IMensajesColaRepository>();

                // 1. Capturar IDs actualmente en caché (fuera del lock para no bloquear durante IO)
                IReadOnlyCollection<long> idsEnCache;
                lock (this._mensajesCache)
                    idsEnCache = this._mensajesCache.Select(m => m.Id).ToList();

                // 2. Verificar cuáles siguen siendo Pendiente/Fallido → detecta los ya procesados
                HashSet<long> idsAunValidos = idsEnCache.Any()
                    ? await mensajesColaRepository.ObtenerIdsPendientesEnListaAsync(traceId, idsEnCache)
                    : new HashSet<long>();

                // 3. Cargar el siguiente lote (excluye los ya en caché para avanzar en la cola)
                List<MensajeColaEntity> mensajesNuevos = await mensajesColaRepository.ObtenerMensajesPendientesAsync(traceId, idsEnCache);

                // 4. Actualizar caché: limpiar procesados + agregar nuevos respetando el máximo
                List<MensajeColaEntity> mensajesParaNotificar;
                lock (this._mensajesCache)
                {
                    this._mensajesCache.RemoveAll(m => !idsAunValidos.Contains(m.Id));
                    int espacioDisponible = MAX_TAMANIO_CACHE - this._mensajesCache.Count;
                    mensajesParaNotificar = mensajesNuevos.Take(espacioDisponible).ToList();
                    this._mensajesCache.AddRange(mensajesParaNotificar);
                }

                if (mensajesParaNotificar.Any())
                    await NotificarNuevosMensajesAsync(traceId, mensajesParaNotificar);
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
        /// Obtiene mensajes por nombre de cola desde el caché en memoria.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <param name="nombreCola">Nombre de la cola a buscar.</param>
        /// <returns>Lista de mensajes encontrados en la cola especificada.</returns>
        public async Task<List<MensajeColaEntity>> ObtenerMensajesNombreColaAsync(string traceId, string nombreCola)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (string.IsNullOrEmpty(nombreCola)) throw new ArgumentException(MENSAJE_ERROR_NOMBRE_COLA_INVALIDO);
                var cola = await this._colaService.ObtenerColaPorNombreAsync(traceId, nombreCola);
                if (cola is null) throw new ArgumentException(MENSAJE_ERROR_NOMBRE_COLA_INVALIDO);
                lock (this._mensajesCache)
                {
                    return this._mensajesCache.Where(x => x.ColaId == cola.Id).ToList();
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
        /// Cuenta la cantidad de mensajes pendientes y fallidos en el sistema.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <returns>Número de mensajes pendientes y fallidos.</returns>
        public async Task<int> ContarMensajesPendientesAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                int totalMensajes = await consultarRepository.ContarAsync<MensajeColaEntity>(
                    traceId, mensaje => mensaje.EstadoId == (int)EstadoMensajeModel.Pendiente ||
                                       mensaje.EstadoId == (int)EstadoMensajeModel.Fallido);

                return totalMensajes;
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
        /// Notifica a los suscriptores sobre nuevos mensajes agrupados por cola.
        /// Busca la cola primero en caché y luego en base de datos para cubrir colas recién creadas.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        /// <param name="nuevos">Lista de entidades de mensajes nuevos a notificar.</param>
        public async Task NotificarNuevosMensajesAsync(string traceId, List<MensajeColaEntity> nuevos)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                var porCola = nuevos.GroupBy(m => m.ColaId);
                foreach (var grupo in porCola)
                {
                    var cola = await this._colaService.ObtenerColaPorIdAsync(traceId, grupo.Key);
                    if (cola is null) continue;
                    List<MensajeColasRespuestaModel> modelos = grupo.Select(e => MapEntityToRespuestaModel(e)).ToList();
                    this._suscripcionColaService.NotificarMensajes(cola.Nombre, modelos);
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
        /// Convierte una entidad de mensaje de cola al modelo de respuesta del dominio.
        /// </summary>
        /// <param name="entity">Entidad de mensaje de cola a convertir.</param>
        /// <returns>Modelo de respuesta mapeado.</returns>
        public static MensajeColasRespuestaModel MapEntityToRespuestaModel(MensajeColaEntity entity) =>
            new MensajeColasRespuestaModel
            {
                Id = entity.Id,
                IdRpc = entity.IdRpc,
                Cola = entity.ColaId,
                TraceId = entity.TraceId,
                Mensaje = entity.ContenidoMensaje,
                Estado = entity.EstadoId,
                MetaDatos = entity.Metadatos,
                TraceIdDiferente = TRACE_ID_DIFERENTE_INICIAL
            };
    }
}
