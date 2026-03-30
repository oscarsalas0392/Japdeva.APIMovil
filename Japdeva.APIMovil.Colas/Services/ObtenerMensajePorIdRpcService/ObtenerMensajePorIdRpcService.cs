using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Colas.Models;
using Japdeva.APIMovil.Colas.Services.ActualizarMensajeExitosoService;
using Japdeva.APIMovil.Colas.Services.ColaService;
using Japdeva.APIMovil.Colas.Services.MensajeColaService;
using Japdeva.APIMovil.Colas.Services.SuscripcionColaService;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;

namespace Japdeva.APIMovil.Colas.Services.ObtenerMensajePorIdRpcService
{
    /// <summary>
    /// Servicio para obtener mensajes de cola por su identificador RPC.
    /// Utiliza caché en memoria y notificación push para evitar polling a la base de datos.
    /// </summary>
    public class ObtenerMensajePorIdRpcService : IObtenerMensajePorIdRpcService
    {
        private readonly ILogger<ObtenerMensajePorIdRpcService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IColaService _colaService;
        private readonly IActualizarMensajeExitosoService _actualizarMensajeExitosoService;
        private readonly ISuscripcionColaService _suscripcionColaService;
        private readonly IMensajeColaService _mensajeColaService;

        private const string MENSAJE_NO_ENCONTRADO = "No se encontró ningún mensaje con IdRpc: ";
        private const string COLA_NO_ENCONTRADA = "No se encontró ninguna cola con nombre: ";
        private const string NOMBRE_COLA_REQUERIDO = "El nombre de la cola es requerido.";
        private const string IDRPC_REQUERIDO = "El IdRpc del mensaje es requerido.";
        private const bool TRACE_ID_DIFERENTE = false;
        private const int TIMEOUT_ESPERA_SEGUNDOS = 30;

        /// <summary>
        /// Inicializa una nueva instancia de la clase ObtenerMensajePorIdRpcService.
        /// </summary>
        /// <param name="logger">Logger para el registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para la inyección de dependencias.</param>
        /// <param name="colaService">Servicio para operaciones de cola.</param>
        /// <param name="actualizarMensajeExitosoService">Servicio para actualizar mensajes exitosos.</param>
        /// <param name="suscripcionColaService">Servicio de suscripciones para espera push por IdRpc.</param>
        /// <param name="mensajeColaService">Servicio de mensajes para búsqueda en caché.</param>
        public ObtenerMensajePorIdRpcService(
            ILogger<ObtenerMensajePorIdRpcService> logger,
            IServiceProvider serviceProvider,
            IColaService colaService,
            IActualizarMensajeExitosoService actualizarMensajeExitosoService,
            ISuscripcionColaService suscripcionColaService,
            IMensajeColaService mensajeColaService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._colaService = colaService;
            this._actualizarMensajeExitosoService = actualizarMensajeExitosoService;
            this._suscripcionColaService = suscripcionColaService;
            this._mensajeColaService = mensajeColaService;
        }

        /// <summary>
        /// Busca directamente en la base de datos un mensaje por su IdRpc y nombre de cola.
        /// </summary>
        /// <param name="traceId">Identificador de seguimiento.</param>
        /// <param name="nombreCola">Nombre de la cola.</param>
        /// <param name="idRpc">Identificador RPC del mensaje.</param>
        /// <returns>El modelo del mensaje encontrado.</returns>
        public async Task<MensajeColasRespuestaModel> BuscarIdRpcAsync(string traceId, string nombreCola, string idRpc)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (string.IsNullOrWhiteSpace(nombreCola))
                    throw new ArgumentException(NOMBRE_COLA_REQUERIDO, nameof(nombreCola));
                if (string.IsNullOrWhiteSpace(idRpc))
                    throw new ArgumentException(IDRPC_REQUERIDO, nameof(idRpc));

                using IServiceScope scope = this._serviceProvider.CreateScope();
                IConsultarRepository consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                ColaEntity? cola = await this._colaService.ObtenerColaPorNombreAsync(traceId, nombreCola);
                if (cola is null) throw new InvalidOperationException($"{COLA_NO_ENCONTRADA}{nombreCola}");
                MensajeColaEntity? mensajeEntity = await consultarRepository.ConsultarAsync<MensajeColaEntity>(
                    traceId, x => x.IdRpc == idRpc && x.ColaId == cola.Id);
                if (mensajeEntity is null) throw new InvalidOperationException($"{MENSAJE_NO_ENCONTRADO}{idRpc}");
                return new MensajeColasRespuestaModel
                {
                    Id = mensajeEntity.Id,
                    Cola = mensajeEntity.ColaId,
                    Mensaje = mensajeEntity.ContenidoMensaje,
                    Estado = mensajeEntity.EstadoId,
                    TraceId = mensajeEntity.TraceId,
                    TraceIdDiferente = TRACE_ID_DIFERENTE,
                    IdRpc = mensajeEntity.IdRpc
                };
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
        /// Espera y obtiene un mensaje por su IdRpc sin polling a la base de datos.
        /// Primero revisa el caché en memoria; si no está, espera la notificación push del BackgroundService.
        /// </summary>
        /// <param name="traceId">Identificador de seguimiento.</param>
        /// <param name="idRpc">Identificador RPC del mensaje.</param>
        /// <param name="nombreCola">Nombre de la cola.</param>
        /// <param name="cancellationToken">Token de cancelación para respetar el timeout del cliente gRPC.</param>
        /// <returns>Resultado HTTP con el mensaje encontrado, o NotFound si se cancela la espera.</returns>
        public async Task<IActionResult> ObtenerMensajeRpcAsync(string traceId, string idRpc, string nombreCola, CancellationToken cancellationToken)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            IActionResult resultado = new NotFoundObjectResult($"{MENSAJE_NO_ENCONTRADO}{idRpc}");
            using CancellationTokenSource localCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            localCts.CancelAfter(TimeSpan.FromSeconds(TIMEOUT_ESPERA_SEGUNDOS));
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                void DispararActualizarExitoso(long id, string traceIdMensaje)
                {
                    ActualizarMensajeSolicitudModel modelo = new ActualizarMensajeSolicitudModel { Id = id, TraceId = traceIdMensaje };
                    _ = Task.Run(
                        async () => await this._actualizarMensajeExitosoService.ActualizarMensajeExitosoAsync(traceId, modelo),
                        cancellationToken);
                }

                // 1. Registrar espera ANTES de revisar caché (evita race condition)
                Task<MensajeColasRespuestaModel?> tareaEspera =
                    this._suscripcionColaService.EsperarMensajePorIdRpcAsync(idRpc, localCts.Token);

                // 2. Buscar en caché (sin consulta a BD)
                MensajeColasRespuestaModel? enCache =
                    await this._mensajeColaService.BuscarEnCachePorIdRpcAsync(traceId, nombreCola, idRpc);
                if (enCache is not null)
                {
                    await localCts.CancelAsync();
                    DispararActualizarExitoso(enCache.Id, enCache.TraceId);
                    resultado = new OkObjectResult(enCache);
                    return resultado;
                }

                // 3. Esperar notificación push del BackgroundService (0 consultas a BD)
                MensajeColasRespuestaModel? mensajeNotificado = await tareaEspera;
                if (mensajeNotificado is not null)
                {
                    DispararActualizarExitoso(mensajeNotificado.Id, mensajeNotificado.TraceId);
                    resultado = new OkObjectResult(mensajeNotificado);
                }
            }
            catch (OperationCanceledException)
            {
                // resultado ya es NotFoundObjectResult por defecto
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
            return resultado;
        }
    }
}
