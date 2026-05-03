using System.Text.Json;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Services.ColaRpcService;
using Japdeva.APIMovil.Common.Services.ColasGrpcClientService;
using Japdeva.APIMovil.Reclamos.Models;

namespace Japdeva.APIMovil.Reclamos.Services.NotificarDepartamentoService
{
    /// <summary>
    /// Servicio que notifica a todos los usuarios de un departamento cuando se ingresa un nuevo reclamo.
    /// Obtiene los correos del departamento, el nombre del usuario y la plantilla en paralelo,
    /// luego publica un correo por cada dirección en la cola de envío.
    /// </summary>
    public class NotificarDepartamentoService : INotificarDepartamentoService
    {
        private readonly ILogger<NotificarDepartamentoService> _logger;
        private readonly IColaRpcService _colaRpcService;
        private readonly IColasGrpcClientService _colasGrpcClientService;
        private const int TIMEOUT_SEGUNDOS = 10;
        private const int PRIORIDAD_ALTA = 1;
        private const string COLA_OBTENER_CORREOS_DEPARTAMENTO = "ObtenerCorreosPorDepartamento";
        private const string COLA_OBTENER_PLANTILLA = "ObtenerPlantilla";
        private const string COLA_OBTENER_USUARIO = "ObtenerUsuario";
        private const string COLA_RESPUESTA = "Respuesta";
        private const string COLA_ENVIAR_CORREO = "EnviarCorreo";
        private const string ID_PLANTILLA_DEPARTAMENTO = "3";
        private const string VARIABLE_NOMBRE = "{{NOMBRE}}";
        private const string VARIABLE_ID_RECLAMO = "{{ID_RECLAMO}}";
        private const string VARIABLE_ANIO = "{{ANIO}}";
        private const int SIN_CORREOS = 0;

        /// <summary>
        /// Inicializa una nueva instancia de NotificarDepartamentoService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="colaRpcService">Servicio RPC para comunicación con colas con espera de respuesta.</param>
        /// <param name="colasGrpcClientService">Cliente gRPC para publicación de mensajes en colas.</param>
        public NotificarDepartamentoService(
            ILogger<NotificarDepartamentoService> logger,
            IColaRpcService colaRpcService,
            IColasGrpcClientService colasGrpcClientService)
        {
            this._logger = logger;
            this._colaRpcService = colaRpcService;
            this._colasGrpcClientService = colasGrpcClientService;
        }

        /// <summary>
        /// Obtiene los correos del departamento, el nombre del usuario y la plantilla en paralelo,
        /// luego publica un correo por cada dirección en la cola de envío.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="idDepartamento">Identificador del departamento a notificar.</param>
        /// <param name="idReclamo">Identificador del reclamo recién ingresado.</param>
        /// <param name="idUsuarioExterno">Identificador del usuario externo que creó el reclamo.</param>
        public async Task NotificarNuevoReclamoAsync(string traceId, long idDepartamento, long idReclamo, long idUsuarioExterno)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TIMEOUT_SEGUNDOS));

                var tareaCorreos = this._colaRpcService.EnviarYEsperarRespuestaAsync(
                    traceId, COLA_OBTENER_CORREOS_DEPARTAMENTO, COLA_RESPUESTA, idDepartamento.ToString(), cts.Token);

                var tareaPlantilla = this._colaRpcService.EnviarYEsperarRespuestaAsync(
                    traceId, COLA_OBTENER_PLANTILLA, COLA_RESPUESTA, ID_PLANTILLA_DEPARTAMENTO, cts.Token);

                var tareaUsuario = this._colaRpcService.EnviarYEsperarRespuestaAsync(
                    traceId, COLA_OBTENER_USUARIO, COLA_RESPUESTA, idUsuarioExterno.ToString(), cts.Token);

                await Task.WhenAll(tareaCorreos, tareaPlantilla, tareaUsuario);

                var mensajeCorreos = tareaCorreos.Result;
                var mensajePlantilla = tareaPlantilla.Result;
                var mensajeUsuario = tareaUsuario.Result;

                if (mensajeCorreos is null || mensajePlantilla is null || mensajeUsuario is null) return;

                var correosDepartamento = JsonSerializer.Deserialize<CorreosDepartamentoRespuestaModel>(mensajeCorreos.Contenido);
                var plantilla = JsonSerializer.Deserialize<PlantillaRespuestaModel>(mensajePlantilla.Contenido);
                var usuarioDatos = JsonSerializer.Deserialize<UsuarioDatosRespuestaModel>(mensajeUsuario.Contenido);

                if (correosDepartamento is null || plantilla is null || usuarioDatos is null) return;
                if (correosDepartamento.Correos.Count == SIN_CORREOS) return;

                string idReclamoStr = idReclamo.ToString();
                string cuerpo = plantilla.Plantilla
                    .Replace(VARIABLE_NOMBRE, usuarioDatos.Nombre)
                    .Replace(VARIABLE_ID_RECLAMO, idReclamoStr)
                    .Replace(VARIABLE_ANIO, DateTime.UtcNow.Year.ToString());

                string asunto = plantilla.Asunto.Replace(VARIABLE_ID_RECLAMO, idReclamoStr);

                IEnumerable<Task> envios = correosDepartamento.Correos.Select(correo =>
                {
                    EnviarCorreoSolicitudModel solicitud = new()
                    {
                        Destinatario = correo,
                        Asunto = asunto,
                        Cuerpo = cuerpo,
                        EsCuerpoHtml = plantilla.EsHtml
                    };
                    return this._colasGrpcClientService.PublicarMensajeAsync(
                        traceId, COLA_ENVIAR_CORREO, JsonSerializer.Serialize(solicitud), Guid.NewGuid().ToString(), PRIORIDAD_ALTA);
                });

                await Task.WhenAll(envios);
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
