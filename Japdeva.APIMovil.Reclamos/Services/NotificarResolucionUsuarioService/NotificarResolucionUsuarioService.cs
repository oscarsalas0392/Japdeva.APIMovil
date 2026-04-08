using System.Text.Json;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Services.ColaRpcService;
using Japdeva.APIMovil.Common.Services.ColasGrpcClientService;
using Japdeva.APIMovil.Reclamos.Models;

namespace Japdeva.APIMovil.Reclamos.Services.NotificarResolucionUsuarioService
{
    /// <summary>
    /// Servicio que notifica al usuario externo el resultado final de su reclamo.
    /// Obtiene los datos del usuario y la plantilla en paralelo, construye el cuerpo del correo
    /// con la descripción y estado de la resolución, y lo publica en la cola de envío.
    /// </summary>
    public class NotificarResolucionUsuarioService : INotificarResolucionUsuarioService
    {
        private readonly ILogger<NotificarResolucionUsuarioService> _logger;
        private readonly IColaRpcService _colaRpcService;
        private readonly IColasGrpcClientService _colasGrpcClientService;
        private const int TIMEOUT_SEGUNDOS = 10;
        private const int PRIORIDAD_ALTA = 1;
        private const string COLA_OBTENER_USUARIO = "ObtenerUsuario";
        private const string COLA_OBTENER_PLANTILLA = "ObtenerPlantilla";
        private const string COLA_RESPUESTA = "Respuesta";
        private const string COLA_ENVIAR_CORREO = "EnviarCorreo";
        private const string ID_PLANTILLA_RESOLUCION = "4";
        private const string VARIABLE_NOMBRE = "{{NOMBRE}}";
        private const string VARIABLE_ID_RECLAMO = "{{ID_RECLAMO}}";
        private const string VARIABLE_DESCRIPCION_RESOLUCION = "{{DESCRIPCION_RESOLUCION}}";
        private const string VARIABLE_ESTADO = "{{ESTADO}}";
        private const string VARIABLE_ANIO = "{{ANIO}}";

        /// <summary>
        /// Inicializa una nueva instancia de NotificarResolucionUsuarioService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="colaRpcService">Servicio RPC para comunicación con colas con espera de respuesta.</param>
        /// <param name="colasGrpcClientService">Cliente gRPC para publicación de mensajes en colas.</param>
        public NotificarResolucionUsuarioService(
            ILogger<NotificarResolucionUsuarioService> logger,
            IColaRpcService colaRpcService,
            IColasGrpcClientService colasGrpcClientService)
        {
            this._logger = logger;
            this._colaRpcService = colaRpcService;
            this._colasGrpcClientService = colasGrpcClientService;
        }

        /// <summary>
        /// Obtiene los datos del usuario y la plantilla de resolución en paralelo, construye el correo
        /// con la descripción y estado final, y lo publica en la cola de envío.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="idReclamo">Identificador del reclamo resuelto.</param>
        /// <param name="idUsuarioExterno">Identificador del usuario externo propietario del reclamo.</param>
        /// <param name="descripcionResolucion">Descripción de la resolución aplicada al reclamo.</param>
        /// <param name="estado">Estado final del reclamo (ej. Completado, Rechazado).</param>
        public async Task NotificarResolucionAsync(string traceId, long idReclamo, long idUsuarioExterno, string descripcionResolucion, string estado)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TIMEOUT_SEGUNDOS));

                var tareaUsuario = this._colaRpcService.EnviarYEsperarRespuestaAsync(
                    traceId, COLA_OBTENER_USUARIO, COLA_RESPUESTA, idUsuarioExterno.ToString(), cts.Token);

                var tareaPlantilla = this._colaRpcService.EnviarYEsperarRespuestaAsync(
                    traceId, COLA_OBTENER_PLANTILLA, COLA_RESPUESTA, ID_PLANTILLA_RESOLUCION, cts.Token);

                await Task.WhenAll(tareaUsuario, tareaPlantilla);

                var mensajeUsuario = tareaUsuario.Result;
                var mensajePlantilla = tareaPlantilla.Result;

                if (mensajeUsuario is null || mensajePlantilla is null) return;

                var usuarioDatos = JsonSerializer.Deserialize<UsuarioDatosRespuestaModel>(mensajeUsuario.Contenido);
                var plantilla = JsonSerializer.Deserialize<PlantillaRespuestaModel>(mensajePlantilla.Contenido);

                if (usuarioDatos is null || plantilla is null) return;

                string idReclamoStr = idReclamo.ToString();
                string cuerpo = plantilla.Plantilla
                    .Replace(VARIABLE_NOMBRE, usuarioDatos.Nombre)
                    .Replace(VARIABLE_ID_RECLAMO, idReclamoStr)
                    .Replace(VARIABLE_DESCRIPCION_RESOLUCION, descripcionResolucion)
                    .Replace(VARIABLE_ESTADO, estado)
                    .Replace(VARIABLE_ANIO, DateTime.UtcNow.Year.ToString());

                EnviarCorreoSolicitudModel solicitudCorreo = new()
                {
                    Destinatario = usuarioDatos.Correo,
                    Asunto = plantilla.Asunto.Replace(VARIABLE_ID_RECLAMO, idReclamoStr),
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
