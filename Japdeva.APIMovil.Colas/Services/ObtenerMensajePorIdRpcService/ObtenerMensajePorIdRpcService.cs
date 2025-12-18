using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Colas.Models;
using Japdeva.APIMovil.Colas.Services.ActualizarMensajeExitosoService;
using Japdeva.APIMovil.Colas.Services.ColaService;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;


namespace Japdeva.APIMovil.Colas.Services.ObtenerMensajePorIdRpcService
{
    /// <summary>
    /// Servicio para obtener mensajes de cola por su identificador RPC
    /// </summary>
    public class ObtenerMensajePorIdRpcService : IObtenerMensajePorIdRpcService
    { 
        private readonly ILogger<ObtenerMensajePorIdRpcService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IColaService _colaService;
        private readonly IActualizarMensajeExitosoService _actualizarMensajeExitosoService;
        
        private const string MENSAJE_NO_ENCONTRADO = "No se encontró ningún mensaje con IdRpc: ";
        private const string COLA_NO_ENCONTRADA = "No se encontró ninguna cola con nombre: ";
        private const string NOMBRE_COLA_REQUERIDO = "El nombre de la cola es requerido.";
        private const string IDRPC_REQUERIDO = "El IdRpc del mensaje es requerido."; 
        private const int INTERVALO_POLLING_MILISEGUNDOS = 500;
        private const int MAX_INTENTOS = 60;
        private const int INTENTOS_INICIALES = 0;
        private const bool TRACE_ID_DIFERENTE = false;

        /// <summary>
        /// Inicializa una nueva instancia de la clase ObtenerMensajePorIdRpcService
        /// </summary>
        /// <param name="logger">Logger para el registro de eventos</param>
        /// <param name="serviceProvider">Proveedor de servicios para la inyección de dependencias</param>
        /// <param name="colaService">Servicio para operaciones de cola</param>
        /// <param name="actualizarMensajeExitosoService">Servicio para actualizar mensajes exitosos</param>
        public ObtenerMensajePorIdRpcService(ILogger<ObtenerMensajePorIdRpcService> logger, IServiceProvider serviceProvider, IColaService colaService, IActualizarMensajeExitosoService actualizarMensajeExitosoService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _colaService = colaService;
            _actualizarMensajeExitosoService = actualizarMensajeExitosoService;
        }

        /// <summary>
        /// Obtiene un mensaje de cola por su identificador RPC
        /// </summary>
        /// <param name="traceId">Identificador de seguimiento</param>
        /// <param name="nombreCola">Nombre de la cola</param>
        /// <param name="idRpc">Identificador RPC del mensaje</param>
        /// <returns>Resultado de la operación con el mensaje encontrado o error si no existe</returns>
        public async Task<MensajeColasRespuestaModel> BuscarIdRpcAsync(string traceId, string nombreCola, string idRpc)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (string.IsNullOrWhiteSpace(nombreCola))
                {
                    throw new ArgumentException(NOMBRE_COLA_REQUERIDO, nameof(nombreCola));
                }

                if (string.IsNullOrWhiteSpace(idRpc))
                {
                    throw new ArgumentException(IDRPC_REQUERIDO, nameof(idRpc));
                }
                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var cola = await this._colaService.ObtenerColaPorNombreAsync(traceId, nombreCola);
                if (cola is null) throw new Exception($"{COLA_NO_ENCONTRADA}{nombreCola}");
                var mensajeEntity = await consultarRepository.ConsultarAsync<MensajeColaEntity>(traceId, x => x.IdRpc == idRpc && x.ColaId == cola.Id);
                if (mensajeEntity is null) throw new Exception($"{MENSAJE_NO_ENCONTRADO}{idRpc}");
                MensajeColasRespuestaModel mensajeColasRespuestaModel = new MensajeColasRespuestaModel();
                mensajeColasRespuestaModel.Id = mensajeEntity.Id;
                mensajeColasRespuestaModel.Cola = mensajeEntity.ColaId;
                mensajeColasRespuestaModel.Mensaje = mensajeEntity.ContenidoMensaje;
                mensajeColasRespuestaModel.Estado = mensajeEntity.EstadoId;
                mensajeColasRespuestaModel.TraceId = mensajeEntity.TraceId;
                mensajeColasRespuestaModel.TraceIdDiferente = TRACE_ID_DIFERENTE;
                mensajeColasRespuestaModel.IdRpc = mensajeEntity.IdRpc;
                return mensajeColasRespuestaModel;
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
        /// Espera y obtiene un mensaje de cola por su identificador RPC con reintentos
        /// </summary>
        /// <param name="traceId">Identificador de seguimiento</param>
        /// <param name="idRpc">Identificador RPC del mensaje</param>
        /// <param name="nombreCola">Nombre de la cola</param>
        /// <returns>Resultado HTTP con el mensaje encontrado o NotFound si no existe después de los reintentos</returns>
        public async Task<IActionResult> ObtenerMensajeRpcAsync(string traceId, string idRpc, string nombreCola)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            int intentos = INTENTOS_INICIALES;
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                while (intentos < MAX_INTENTOS)
                {
                    try
                    {
                        var mensaje = await BuscarIdRpcAsync(traceId, nombreCola, idRpc);
                        if (mensaje is not null)
                        {
                            _ = Task.Run(async () =>
                            {
                                ActualizarMensajeSolicitudModel actualizarMensajeSolicitudModel = new ActualizarMensajeSolicitudModel();
                                actualizarMensajeSolicitudModel.Id = mensaje.Id;
                                actualizarMensajeSolicitudModel.TraceId = mensaje.TraceId;
                                await this._actualizarMensajeExitosoService.ActualizarMensajeExitosoAsync(traceId, actualizarMensajeSolicitudModel);
                            });
                            return new OkObjectResult(mensaje);
                        }
                    }
                    catch (Exception)
                    {
                        // Ignorar excepciones y continuar con el reintento
                    }
                    intentos++;
                    await Task.Delay(INTERVALO_POLLING_MILISEGUNDOS);
                }
                return new NotFoundObjectResult($"{MENSAJE_NO_ENCONTRADO}{idRpc}");
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