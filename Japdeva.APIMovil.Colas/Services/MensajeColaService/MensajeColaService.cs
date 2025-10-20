using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Colas.Models;
using Japdeva.APIMovil.Colas.Services.ColaService;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;

namespace Japdeva.APIMovil.Colas.Services.MensajeColaService
{
    /// <summary>
    /// Servicio para gestionar mensajes en colas del sistema.
    /// </summary>
    public class MensajeColaService : IMensajeColaService
    {
        private readonly IConsultarListaRepository _consultarListaRepository;
        private readonly IConsultarRepository _consultarRepository;
        private readonly IColaService _colaService;
        private readonly ILogger<MensajeColaService> _logger;
        private readonly List<MensajeColaEntity> _mensajesCache = new List<MensajeColaEntity>();
        private const string MENSAJE_ERROR_NOMBRE_COLA_INVALIDO = "El nombre de cola proporcionado no es válido.";
        private const int PAGINA_INICIAL = 1;
        

        /// <summary>
        /// Inicializa una nueva instancia de la clase MensajeColaService.
        /// </summary>
        /// <param name="consultarListaRepository">Repositorio para consultas de listas de datos.</param>
        /// <param name="consultarRepository">Repositorio para consultas individuales.</param>
        /// <param name="logger">Instancia de logger para registrar eventos.</param>
        /// <param name="colaService">Servicio para gestionar operaciones de colas.</param>
        public MensajeColaService(
            IConsultarListaRepository consultarListaRepository,
            IConsultarRepository consultarRepository,
            ILogger<MensajeColaService> logger, IColaService colaService)
        {
            this._consultarListaRepository = consultarListaRepository;
            this._consultarRepository = consultarRepository;
            this._logger = logger;
            this._colaService = colaService;
        }

        /// <summary>
        /// Llena el caché con mensajes pendientes y fallidos.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad para el seguimiento de la operación.</param>
        public async Task LlenarCacheMensajesAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                int pagina = PAGINA_INICIAL;
                var mensajes = await this._consultarListaRepository.ConsultarListaAsync<MensajeColaEntity>(
                traceId, pagina, mensaje => mensaje.EstadoId == (int)EstadoMensajeModel.Pendiente ||
                                          mensaje.EstadoId == (int)EstadoMensajeModel.Fallido);

                if (mensajes is null || !mensajes.Lista.Any()) return;

                lock (this._mensajesCache)
                {
                    this._mensajesCache.Clear();
                    this._mensajesCache.AddRange(mensajes.Lista);
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
        /// Obtiene mensajes por nombre de cola.
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
                return this._mensajesCache.Where(x => x.Id == cola.Id).ToList();
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

                int totalMensajes = await this._consultarRepository.ContarAsync<MensajeColaEntity>(
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

    }
}