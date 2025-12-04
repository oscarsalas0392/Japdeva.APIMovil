using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Colas.Models;
using Japdeva.APIMovil.Colas.Repositories.MensajesColaRepository;
using Japdeva.APIMovil.Colas.Services.ColaService;
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
        private readonly List<MensajeColaEntity> _mensajesCache = new List<MensajeColaEntity>();
        private const string MENSAJE_ERROR_NOMBRE_COLA_INVALIDO = "El nombre de cola proporcionado no es válido.";

        /// <summary>
        /// Inicializa una nueva instancia de la clase MensajeColaService.
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias.</param>
        /// <param name="logger">Instancia de logger para registrar eventos.</param>
        /// <param name="colaService">Servicio para gestionar operaciones de colas.</param>
        public MensajeColaService(
            IServiceProvider serviceProvider,
            ILogger<MensajeColaService> logger, IColaService colaService)
        {
            this._logger = logger;
            this._colaService = colaService;
            this._serviceProvider = serviceProvider;
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

                using var scope = this._serviceProvider.CreateScope();
                var mensajesColaRepository = scope.ServiceProvider.GetRequiredService<IMensajesColaRepository>();
                var mensajes = await mensajesColaRepository.ObtenerMensajesPendientesAsync(traceId);

                if (mensajes is null || !mensajes.Any()) return;

                lock (this._mensajesCache)
                {
                    this._mensajesCache.Clear();
                    this._mensajesCache.AddRange(mensajes);
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

    }
}