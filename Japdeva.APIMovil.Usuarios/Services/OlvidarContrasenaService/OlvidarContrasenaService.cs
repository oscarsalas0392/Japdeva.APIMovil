using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Common.Services.ColaRpcService;
using Japdeva.APIMovil.Common.Services.ColasGrpcClientService;
using Japdeva.APIMovil.Usuarios.Entities;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.Services.OlvidarContrasenaService
{
    /// <summary>
    /// Servicio para la recuperación de contraseña mediante generación de clave temporal.
    /// </summary>
    public class OlvidarContrasenaService : IOlvidarContrasenaService
    {
        private readonly ILogger<OlvidarContrasenaService> _logger;
        private readonly IColaRpcService _colaRpcService;
        private readonly IColasGrpcClientService _colasGrpcClientService;
        private readonly IServiceProvider _serviceProvider;
        private const string MENSAJE_CORREO_REQUERIDO = "El correo es requerido.";
        private const string MENSAJE_CORREO_INVALIDO = "El formato del correo no es válido.";
        private const string MENSAJE_ERROR_PLANTILLA_NO_OBTENIDA = "No se pudo obtener la plantilla de correo desde la cola.";
        private const string MENSAJE_ERROR_PLANTILLA_INVALIDA = "La respuesta de la plantilla de correo no pudo ser deserializada.";
        private const string PATRON_CORREO = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        private const string CARACTERES_CONTRASENA = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        private const string VARIABLE_NOMBRE = "{{NOMBRE}}";
        private const string VARIABLE_CONTRASENA_TEMPORAL = "{{CONTRASENA_TEMPORAL}}";
        private const string COLA_OBTENER_PLANTILLA = "ObtenerPlantilla";
        private const string COLA_RESPUESTA = "Respuesta";
        private const string COLA_ENVIAR_CORREO = "EnviarCorreo";
        private const string ASUNTO_CORREO = "Recuperación de contraseña";
        private const string ID_PLANTILLA = "1";
        private const int LONGITUD_CONTRASENA_TEMPORAL = 10;
        private const int INDICE_INICIAL = 0;
        private const int PRIORIDAD_ALTA = 1;
        private const bool ES_CUERPO_HTML = true;
        private const string PRUEBA = "oscar.salas03@gmail.com";

        /// <summary>
        /// Inicializa una nueva instancia de OlvidarContrasenaService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias.</param>
        /// <param name="colaRpcService">Servicio RPC para publicar mensajes en la cola con espera de respuesta.</param>
        /// <param name="colasGrpcClientService">Cliente gRPC de colas para publicación directa sin espera.</param>
        public OlvidarContrasenaService(
            ILogger<OlvidarContrasenaService> logger,
            IServiceProvider serviceProvider,
            IColaRpcService colaRpcService,
            IColasGrpcClientService colasGrpcClientService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this._colaRpcService = colaRpcService;
            this._colasGrpcClientService = colasGrpcClientService;
        }

        /// <summary>
        /// Genera una contraseña temporal, obtiene la plantilla de correo y publica el envío a la cola de EnvioCorreos.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="solicitud">Datos de la solicitud con el correo del usuario.</param>
        /// <returns>Resultado de la operación.</returns>
        public async Task<IActionResult> OlvidarContrasenaAsync(string traceId, OlvidarContrasenaSolicitudModel solicitud)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (solicitud is null) throw new ArgumentNullException(nameof(solicitud));
                if (string.IsNullOrEmpty(solicitud.Correo)) throw new ArgumentException(MENSAJE_CORREO_REQUERIDO);
                if (!Regex.IsMatch(solicitud.Correo, PATRON_CORREO)) throw new ArgumentException(MENSAJE_CORREO_INVALIDO);

                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var actualizarRepository = scope.ServiceProvider.GetRequiredService<IActualizarRepository>();

                var usuario = await consultarRepository.ConsultarAsync<UsuarioEntity>(traceId, u => u.Correo == solicitud.Correo && u.Activo);

                if (usuario is not null)
                {
                    string contrasenaTemporal = this.GenerarContrasenaTemporal(traceId);
                    usuario.Contrasena = contrasenaTemporal;
                    usuario.FechaEdicion = DateTime.UtcNow;

                    var colaMensaje = await this._colaRpcService.EnviarYEsperarRespuestaAsync(traceId, COLA_OBTENER_PLANTILLA, COLA_RESPUESTA, ID_PLANTILLA, CancellationToken.None);
                    if (colaMensaje is null) throw new InvalidOperationException(MENSAJE_ERROR_PLANTILLA_NO_OBTENIDA);

                    var plantillaRespuestaModel = JsonSerializer.Deserialize<PlantillaRespuestaModel>(colaMensaje.Contenido);
                    if (plantillaRespuestaModel is null) throw new InvalidOperationException(MENSAJE_ERROR_PLANTILLA_INVALIDA);

                    string cuerpo = plantillaRespuestaModel.Plantilla;
                    cuerpo = cuerpo.Replace(VARIABLE_NOMBRE, usuario.Nombre);
                    cuerpo = cuerpo.Replace(VARIABLE_CONTRASENA_TEMPORAL, contrasenaTemporal);

                    string contenidoCorreo = JsonSerializer.Serialize(new EnviarCorreoSolicitudModel
                    {
                        Destinatario = PRUEBA,
                        Asunto = ASUNTO_CORREO,
                        Cuerpo = cuerpo,
                        EsCuerpoHtml = ES_CUERPO_HTML
                    });

                    await this._colasGrpcClientService.PublicarMensajeAsync(traceId, COLA_ENVIAR_CORREO, contenidoCorreo, Guid.NewGuid().ToString(), PRIORIDAD_ALTA);

                    await actualizarRepository.ActualizarAsync<UsuarioEntity>(traceId, usuario);
                }

                return new OkResult();
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
        /// Genera una contraseña temporal aleatoria de longitud fija.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <returns>Contraseña temporal generada.</returns>
        public string GenerarContrasenaTemporal(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                char[] contrasena = new char[LONGITUD_CONTRASENA_TEMPORAL];
                for (int i = INDICE_INICIAL; i < LONGITUD_CONTRASENA_TEMPORAL; i++)
                {
                    contrasena[i] = CARACTERES_CONTRASENA[Random.Shared.Next(CARACTERES_CONTRASENA.Length)];
                }
                return new string(contrasena);
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
