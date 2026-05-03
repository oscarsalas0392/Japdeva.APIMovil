using System.Text.Json;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Services.ColaRpcService;
using Japdeva.APIMovil.Common.Services.ColasGrpcClientService;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.Services.NotificarContrasenaTemporalService
{
    /// <summary>
    /// Servicio que obtiene la plantilla de contraseña temporal y publica el correo en la cola de envío.
    /// </summary>
    public class NotificarContrasenaTemporalService : INotificarContrasenaTemporalService
    {
        private readonly ILogger<NotificarContrasenaTemporalService> _logger;
        private readonly IColaRpcService _colaRpcService;
        private readonly IColasGrpcClientService _colasGrpcClientService;
        private const int TIMEOUT_SEGUNDOS = 10;
        private const int PRIORIDAD_ALTA = 1;
        private const string COLA_OBTENER_PLANTILLA = "ObtenerPlantilla";
        private const string COLA_RESPUESTA = "Respuesta";
        private const string COLA_ENVIAR_CORREO = "EnviarCorreo";
        private const string ID_PLANTILLA = "1";
        private const string VARIABLE_NOMBRE = "{{NOMBRE}}";
        private const string VARIABLE_CONTRASENA_TEMPORAL = "{{CONTRASENA_TEMPORAL}}";
        private const string VARIABLE_ANIO = "{{ANIO}}";
        private const string MENSAJE_ERROR_PLANTILLA_NO_OBTENIDA = "No se pudo obtener la plantilla de correo desde la cola.";
        private const string MENSAJE_ERROR_PLANTILLA_INVALIDA = "La respuesta de la plantilla de correo no pudo ser deserializada.";

        /// <summary>
        /// Inicializa una nueva instancia de NotificarContrasenaTemporalService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="colaRpcService">Servicio RPC para comunicación con colas con espera de respuesta.</param>
        /// <param name="colasGrpcClientService">Cliente gRPC para publicación de mensajes en colas.</param>
        public NotificarContrasenaTemporalService(
            ILogger<NotificarContrasenaTemporalService> logger,
            IColaRpcService colaRpcService,
            IColasGrpcClientService colasGrpcClientService)
        {
            this._logger = logger;
            this._colaRpcService = colaRpcService;
            this._colasGrpcClientService = colasGrpcClientService;
        }

        /// <summary>
        /// Obtiene la plantilla de correo y publica el envío con la contraseña temporal generada.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="nombre">Nombre del usuario destinatario.</param>
        /// <param name="correo">Dirección de correo del usuario.</param>
        /// <param name="contrasenaTemporal">Contraseña temporal generada para el usuario.</param>
        public async Task NotificarAsync(string traceId, string nombre, string correo, string contrasenaTemporal)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TIMEOUT_SEGUNDOS));

                var colaMensaje = await this._colaRpcService.EnviarYEsperarRespuestaAsync(
                    traceId, COLA_OBTENER_PLANTILLA, COLA_RESPUESTA, ID_PLANTILLA, cts.Token);

                if (colaMensaje is null) throw new InvalidOperationException(MENSAJE_ERROR_PLANTILLA_NO_OBTENIDA);

                var plantilla = JsonSerializer.Deserialize<PlantillaRespuestaModel>(colaMensaje.Contenido);
                if (plantilla is null) throw new InvalidOperationException(MENSAJE_ERROR_PLANTILLA_INVALIDA);

                string cuerpo = plantilla.Plantilla
                    .Replace(VARIABLE_NOMBRE, nombre)
                    .Replace(VARIABLE_CONTRASENA_TEMPORAL, contrasenaTemporal)
                    .Replace(VARIABLE_ANIO, DateTime.UtcNow.Year.ToString());

                EnviarCorreoSolicitudModel solicitudCorreo = new()
                {
                    Destinatario = correo,
                    Asunto = plantilla.Asunto,
                    Cuerpo = cuerpo,
                    EsCuerpoHtml = plantilla.EsHtml
                };

                await this._colasGrpcClientService.PublicarMensajeAsync(
                    traceId, COLA_ENVIAR_CORREO, JsonSerializer.Serialize(solicitudCorreo), Guid.NewGuid().ToString(), PRIORIDAD_ALTA);
            }
            catch (Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
            }
            finally
            {
                this._logger.Fin(traceId, nombreMetodo);
            }
        }
    }
}
