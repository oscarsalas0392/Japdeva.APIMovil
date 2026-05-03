using System.Collections.Concurrent;
using System.Threading.Channels;
using Japdeva.APIMovil.Colas.Models;
using Japdeva.APIMovil.Common.Extensions;

namespace Japdeva.APIMovil.Colas.Services.SuscripcionColaService
{
    /// <summary>
    /// Servicio para gestionar suscripciones a colas mediante canales en memoria,
    /// permitiendo notificación push de mensajes a los microservicios suscriptores.
    /// </summary>
    public class SuscripcionColaService : ISuscripcionColaService
    {
        private readonly ILogger<SuscripcionColaService> _logger;
        private readonly ConcurrentDictionary<string, List<Channel<MensajeColasRespuestaModel>>> _suscripciones;
        private readonly ConcurrentDictionary<string, TaskCompletionSource<MensajeColasRespuestaModel>> _esperandoPorIdRpc;
        private readonly object _lock = new();
        private const int CAPACIDAD_CANAL = 100;
        private const bool LECTOR_UNICO = true;
        private const bool ESCRITOR_UNICO = false;

        /// <summary>
        /// Inicializa una nueva instancia de SuscripcionColaService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        public SuscripcionColaService(ILogger<SuscripcionColaService> logger)
        {
            this._logger = logger;
            this._suscripciones = new ConcurrentDictionary<string, List<Channel<MensajeColasRespuestaModel>>>(StringComparer.OrdinalIgnoreCase);
            this._esperandoPorIdRpc = new ConcurrentDictionary<string, TaskCompletionSource<MensajeColasRespuestaModel>>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Suscribe un nuevo consumidor a una cola y retorna el canal de lectura.
        /// </summary>
        /// <param name="nombreCola">Nombre de la cola a suscribir.</param>
        /// <returns>Canal de lectura para recibir mensajes en tiempo real.</returns>
        public ChannelReader<MensajeColasRespuestaModel> Suscribir(string nombreCola)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(nombreCola, nombreMetodo);
                Channel<MensajeColasRespuestaModel> canal = Channel.CreateBounded<MensajeColasRespuestaModel>(new BoundedChannelOptions(CAPACIDAD_CANAL)
                {
                    FullMode = BoundedChannelFullMode.DropOldest,
                    SingleReader = LECTOR_UNICO,
                    SingleWriter = ESCRITOR_UNICO
                });
                this._suscripciones.AddOrUpdate(
                    nombreCola,
                    _ => new List<Channel<MensajeColasRespuestaModel>> { canal },
                    (_, lista) =>
                    {
                        lock (this._lock) { lista.Add(canal); }
                        return lista;
                    });
                return canal.Reader;
            }
            catch (Exception ex)
            {
                this._logger.Error(nombreCola, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(nombreCola, nombreMetodo);
            }
        }

        /// <summary>
        /// Elimina la suscripción de un consumidor a una cola.
        /// </summary>
        /// <param name="nombreCola">Nombre de la cola.</param>
        /// <param name="reader">Canal de lectura a desuscribir.</param>
        public void Desuscribir(string nombreCola, ChannelReader<MensajeColasRespuestaModel> reader)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(nombreCola, nombreMetodo);
                if (this._suscripciones.TryGetValue(nombreCola, out var lista))
                {
                    lock (this._lock)
                    {
                        lista.RemoveAll(c => c.Reader == reader);
                    }
                }
            }
            catch (Exception ex)
            {
                this._logger.Error(nombreCola, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(nombreCola, nombreMetodo);
            }
        }

        /// <summary>
        /// Notifica a todos los suscriptores de una cola con nuevos mensajes disponibles.
        /// </summary>
        /// <param name="nombreCola">Nombre de la cola que recibió nuevos mensajes.</param>
        /// <param name="mensajes">Lista de nuevos mensajes a notificar.</param>
        public void NotificarMensajes(string nombreCola, IEnumerable<MensajeColasRespuestaModel> mensajes)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(nombreCola, nombreMetodo);
                List<MensajeColasRespuestaModel> listaMensajes = mensajes.ToList();

                // Notificar a suscriptores del stream (ObtenerMensajesPendientes)
                if (this._suscripciones.TryGetValue(nombreCola, out List<Channel<MensajeColasRespuestaModel>>? lista))
                {
                    List<Channel<MensajeColasRespuestaModel>> copia;
                    lock (this._lock) { copia = lista.ToList(); }
                    foreach (MensajeColasRespuestaModel mensaje in listaMensajes)
                        foreach (Channel<MensajeColasRespuestaModel> canal in copia)
                            canal.Writer.TryWrite(mensaje);
                }

                // Completar esperas puntuales por cola+IdRpc (ObtenerMensajePorIdRpc)
                foreach (MensajeColasRespuestaModel mensaje in listaMensajes)
                {
                    string clave = $"{nombreCola}:{mensaje.IdRpc}";
                    if (!string.IsNullOrEmpty(mensaje.IdRpc) &&
                        this._esperandoPorIdRpc.TryRemove(clave, out TaskCompletionSource<MensajeColasRespuestaModel>? tcs))
                        tcs.TrySetResult(mensaje);
                }
            }
            catch (Exception ex)
            {
                this._logger.Error(nombreCola, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(nombreCola, nombreMetodo);
            }
        }

        /// <summary>
        /// Registra una espera puntual por un mensaje con un IdRpc específico en una cola concreta.
        /// Cuando el BackgroundService notifique ese mensaje en esa cola, la tarea se completa sin polling a BD.
        /// </summary>
        /// <param name="nombreCola">Nombre de la cola donde se espera el mensaje.</param>
        /// <param name="idRpc">Identificador RPC del mensaje esperado.</param>
        /// <param name="cancellationToken">Token de cancelación para respetar el timeout del cliente.</param>
        /// <returns>El mensaje cuando llegue, o null si se cancela la espera.</returns>
        public async Task<MensajeColasRespuestaModel?> EsperarMensajePorIdRpcAsync(string nombreCola, string idRpc, CancellationToken cancellationToken)
        {
            string clave = $"{nombreCola}:{idRpc}";
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(clave, nombreMetodo);
                TaskCompletionSource<MensajeColasRespuestaModel> tcs =
                    new TaskCompletionSource<MensajeColasRespuestaModel>(TaskCreationOptions.RunContinuationsAsynchronously);
                this._esperandoPorIdRpc[clave] = tcs;
                return await tcs.Task.WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException ex)
            {
                this._logger.Error(clave, nombreMetodo, ex);
                throw;
            }
            catch (Exception ex)
            {
                this._logger.Error(clave, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._esperandoPorIdRpc.TryRemove(clave, out _);
                this._logger.Fin(clave, nombreMetodo);
            }
        }
    }
}
