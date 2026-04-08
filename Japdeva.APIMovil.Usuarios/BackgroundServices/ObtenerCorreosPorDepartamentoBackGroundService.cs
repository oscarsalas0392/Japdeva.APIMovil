using System.Text.Json;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Common.Services.ColasGrpcClientService;
using Japdeva.APIMovil.Usuarios.Entities;
using Japdeva.APIMovil.Usuarios.Models;

namespace Japdeva.APIMovil.Usuarios.BackgroundServices
{
    /// <summary>
    /// Servicio de fondo que procesa mensajes de la cola ObtenerCorreosPorDepartamento.
    /// Recibe el ID del departamento y responde con los correos de sus usuarios activos.
    /// </summary>
    public class ObtenerCorreosPorDepartamentoBackGroundService : BackgroundService
    {
        private readonly ILogger<ObtenerCorreosPorDepartamentoBackGroundService> _logger;
        private readonly IColasGrpcClientService _colasGrpcClientService;
        private readonly IServiceProvider _serviceProvider;
        private const int TIEMPO_ESPERA_RECONEXION = 30;
        private const int PAGINA_INICIAL = 1;
        private const int PRIORIDAD_ALTA = 1;
        private const string TRACE_ID_BACKGROUND = "BACKGROUND_OBTENER_CORREOS_DEPARTAMENTO";
        private const string NOMBRE_COLA = "ObtenerCorreosPorDepartamento";
        private const string CLAVE_COLA_RESPUESTA = "colaRespuesta";
        private const string MENSAJE_COLA_RESPUESTA_VACIA = "No se pudo obtener el nombre de la cola de respuesta desde los metadatos.";
        private const string MENSAJE_ERROR_ACTUALIZAR_ESTADO = "Ocurrio un error al actualizar el estado del mensaje de la cola.";

        /// <summary>
        /// Inicializa una nueva instancia del servicio de fondo para obtención de correos por departamento.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="colasGrpcClientService">Cliente gRPC del microservicio de Colas.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolver dependencias con scope.</param>
        public ObtenerCorreosPorDepartamentoBackGroundService(
            ILogger<ObtenerCorreosPorDepartamentoBackGroundService> logger,
            IColasGrpcClientService colasGrpcClientService,
            IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._colasGrpcClientService = colasGrpcClientService;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Ejecuta el loop principal que mantiene el stream abierto con Colas y procesa mensajes en tiempo real.
        /// </summary>
        /// <param name="stoppingToken">Token para detener el servicio de forma controlada.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(TRACE_ID_BACKGROUND, nombreMetodo);

                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        await foreach (ColaMensajeModel cola in this._colasGrpcClientService.ObtenerMensajesPendientesAsync(TRACE_ID_BACKGROUND, NOMBRE_COLA, stoppingToken))
                        {
                            await this.ProcesarMensajeAsync(cola);
                        }
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        this._logger.Error(TRACE_ID_BACKGROUND, nombreMetodo, ex);
                    }

                    await Task.Delay(TIEMPO_ESPERA_RECONEXION, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                this._logger.Error(TRACE_ID_BACKGROUND, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(TRACE_ID_BACKGROUND, nombreMetodo);
            }
        }

        /// <summary>
        /// Procesa un mensaje: consulta las asignaciones del departamento, obtiene los correos y publica la respuesta.
        /// </summary>
        /// <param name="cola">Mensaje recibido desde la cola ObtenerCorreosPorDepartamento.</param>
        public async Task ProcesarMensajeAsync(ColaMensajeModel cola)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            string traceIdMensaje = string.Empty;
            try
            {
                this._logger.Inicio(TRACE_ID_BACKGROUND, nombreMetodo);
                traceIdMensaje = cola.TraceId;

                var mensajeActualizado = await this._colasGrpcClientService.ActualizarMensajeEnProcesoAsync(TRACE_ID_BACKGROUND, cola.Id, traceIdMensaje);
                if (mensajeActualizado is null) throw new Exception(MENSAJE_ERROR_ACTUALIZAR_ESTADO);
                if (mensajeActualizado.TraceIdDiferente) return;
                traceIdMensaje = mensajeActualizado.TraceId;

                int idDepartamento = Convert.ToInt32(cola.Contenido);
                using var scope = this._serviceProvider.CreateScope();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();

                var asignaciones = await consultarListaRepository.ConsultarListaAsync<DepartamentoUsuarioEntity>(
                    traceIdMensaje, PAGINA_INICIAL, a => a.IdDepartamento == idDepartamento && a.Activo);

                List<int> idsUsuarios = asignaciones.Lista.Select(a => a.IdUsuario).ToList();

                var usuarios = await consultarListaRepository.ConsultarListaAsync<UsuarioEntity>(
                    traceIdMensaje, PAGINA_INICIAL, u => idsUsuarios.Contains(u.Id) && u.Activo);

                var metaDatos = JsonSerializer.Deserialize<Dictionary<string, string>>(cola.MetaDatos);
                if (metaDatos is null || !metaDatos.TryGetValue(CLAVE_COLA_RESPUESTA, out string? colaRespuesta) || string.IsNullOrEmpty(colaRespuesta))
                    throw new InvalidOperationException(MENSAJE_COLA_RESPUESTA_VACIA);

                CorreosDepartamentoRespuestaModel respuesta = new()
                {
                    Correos = usuarios.Lista.Select(u => u.Correo).ToList()
                };

                string json = JsonSerializer.Serialize(respuesta);
                Task publicar = this._colasGrpcClientService.PublicarMensajeAsync(TRACE_ID_BACKGROUND, colaRespuesta, json, cola.IdRpc, PRIORIDAD_ALTA, string.Empty);
                Task actualizar = this._colasGrpcClientService.ActualizarMensajeExitosoAsync(TRACE_ID_BACKGROUND, cola.Id, traceIdMensaje);
                await Task.WhenAll(publicar, actualizar);
            }
            catch (Exception ex)
            {
                await this._colasGrpcClientService.ActualizarMensajeFallidoAsync(TRACE_ID_BACKGROUND, cola.Id, traceIdMensaje);
                this._logger.Error(TRACE_ID_BACKGROUND, nombreMetodo, ex);
            }
            finally
            {
                this._logger.Fin(TRACE_ID_BACKGROUND, nombreMetodo);
            }
        }
    }
}
