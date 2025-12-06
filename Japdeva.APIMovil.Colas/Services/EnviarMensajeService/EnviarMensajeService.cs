using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Japdeva.APIMovil.Colas.Entities;
using Japdeva.APIMovil.Colas.Models;
using Japdeva.APIMovil.Colas.Services.ColaService;
using Japdeva.APIMovil.Colas.Services.EstadoMensajeService;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;


namespace Japdeva.APIMovil.Colas.Services.EnviarMensajeService
{
    /// <summary>
    /// Servicio para el envío de mensajes a colas del sistema.
    /// </summary>
    public class EnviarMensajeService : IEnviarMensajeService
    {
        private readonly ILogger<EnviarMensajeService> _logger;
        private readonly IEstadoMensajeService _estadoMensajeService;
        private readonly IColaService _colaService;
        private readonly IServiceProvider _serviceProvider;
        
        private const int NUMERO_REINTENTOS = 3;
        private const int INICIO_REINTENTOS = 0; 
        private const string NUMERO_REINTENTOS_ENV_VAR = "NUMERO_REINTENTOS";
        private const string MENSAJE_ERROR_COLA_VACIO = "La cola no puede estar vacía.";
        private const string MENSAJE_ERROR_MENSAJE_VACIO = "El contenido del mensaje no puede estar vacío.";
        private const string MENSAJE_ERROR_ESTADO_INVALIDO = "El estado del mensaje no es válido.";
        private const bool COLA_ACTIVA = true;
        private const bool TRACE_ID_DIFERENTE = false;
        private const string METADATOS_VACIO = "";


        /// <summary>
        /// Inicializa una nueva instancia de la clase EnviarMensajeService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para la inyección de dependencias.</param>
        /// <param name="colaService">Servicio para gestionar colas.</param>
        /// <param name="estadoMensajeService">Servicio para gestionar estados de mensajes.</param>
        public EnviarMensajeService(ILogger<EnviarMensajeService> logger, IServiceProvider serviceProvider, IColaService colaService, IEstadoMensajeService estadoMensajeService)
        {
            this._logger = logger;
            this._colaService = colaService;
            this._estadoMensajeService = estadoMensajeService;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Envía un mensaje a la cola especificada.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad</param>
        /// <param name="mensaje">Datos del mensaje a enviar</param>
        /// <returns>Resultado de la operación de envío</returns>
        public async Task<IActionResult> EnviarMensajeAsync(string traceId, EnviarMensajeSolicitudModel mensaje)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                if (string.IsNullOrEmpty(mensaje.ContenidoMensaje)) throw new ArgumentException(MENSAJE_ERROR_MENSAJE_VACIO);

                if (string.IsNullOrEmpty(mensaje.NombreCola)) throw new ArgumentException(MENSAJE_ERROR_COLA_VACIO);

                var estadoCola = this._estadoMensajeService.ObtenerEstadoMensajePorId(traceId, (int)EstadoMensajeModel.Pendiente);
                if (estadoCola is null) throw new Exception(MENSAJE_ERROR_ESTADO_INVALIDO);

                var cola = await this._colaService.ObtenerColaPorNombreAsync(traceId, mensaje.NombreCola);
                using var scope = this._serviceProvider.CreateScope();
                if (cola is null)
                {
                    cola = new ColaEntity();
                    cola.Nombre = mensaje.NombreCola;
                    cola.FechaRegistro = DateTime.UtcNow;
                    cola.Activo = COLA_ACTIVA;
                    var agregarRepository = scope.ServiceProvider.GetRequiredService<IAgregarRepository>();
                    await agregarRepository.AgregarAsync<ColaEntity>(traceId, cola);
                }

                bool esNumero = int.TryParse(Environment.GetEnvironmentVariable(NUMERO_REINTENTOS_ENV_VAR), out int reintentos);
                reintentos = esNumero ? reintentos : NUMERO_REINTENTOS;

                var mensajeEntity = new MensajeColaEntity();
                mensajeEntity.ColaId = cola.Id;
                mensajeEntity.ContenidoMensaje = mensaje.ContenidoMensaje;
                mensajeEntity.EstadoId = estadoCola.Id;
                mensajeEntity.PrioridadId = mensaje.Prioridad;
                mensajeEntity.FechaRegistro = DateTime.UtcNow;
                mensajeEntity.Metadatos = JsonSerializer.Serialize(mensaje.Metadatos) ?? METADATOS_VACIO;
                mensajeEntity.ContadorReintentos = INICIO_REINTENTOS;
                mensajeEntity.TraceId = mensaje.TraceId;
                var _agregarRepository = scope.ServiceProvider.GetRequiredService<IAgregarRepository>();
                await _agregarRepository.AgregarAsync<MensajeColaEntity>(traceId, mensajeEntity);

                MensajeColasRespuestaModel mensajeColasRespuestaModel = new MensajeColasRespuestaModel();
                mensajeColasRespuestaModel.Id = mensajeEntity.Id;
                mensajeColasRespuestaModel.Cola = mensajeEntity.ColaId;
                mensajeColasRespuestaModel.Mensaje = mensajeEntity.ContenidoMensaje;
                mensajeColasRespuestaModel.Estado = mensajeEntity.EstadoId;
                mensajeColasRespuestaModel.TraceId = mensajeEntity.TraceId;
                mensajeColasRespuestaModel.TraceIdDiferente = TRACE_ID_DIFERENTE;
                return new OkObjectResult(mensajeColasRespuestaModel);
            }
            catch (ArgumentException ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                throw;
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