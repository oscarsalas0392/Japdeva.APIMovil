using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ActualizarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.EnvioCorreos.Entities;
using Japdeva.APIMovil.EnvioCorreos.Services.SmtpService;

namespace Japdeva.APIMovil.EnvioCorreos.Services.ProcesarCorreosPendientesService
{
    /// <summary>
    /// Servicio para procesar correos pendientes: consulta la cola y realiza el envío por SMTP.
    /// El número máximo de intentos se obtiene de la variable de entorno CORREO_MAX_INTENTOS.
    /// </summary>
    public class ProcesarCorreosPendientesService : IProcesarCorreosPendientesService
    {
        private readonly ILogger<ProcesarCorreosPendientesService> _logger;
        private readonly IConsultarListaRepository _consultarListaRepository;
        private readonly IActualizarRepository _actualizarRepository;
        private readonly ISmtpService _smtpService;
        private readonly int _maxIntentos;
        private const string ENV_MAX_INTENTOS = "CORREO_MAX_INTENTOS";
        private const int MAX_INTENTOS_DEFECTO = 3;
        private const int PAGINA_INICIAL = 1;
        private const int TOTAL_PAGINAS_INICIAL = 0;
        private const bool ENVIADO_EXITOSO = true;

        /// <summary>
        /// Inicializa una nueva instancia de ProcesarCorreosPendientesService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="consultarListaRepository">Repositorio para consultar listas de entidades.</param>
        /// <param name="actualizarRepository">Repositorio para actualizar entidades.</param>
        /// <param name="smtpService">Servicio de envío SMTP.</param>
        public ProcesarCorreosPendientesService(ILogger<ProcesarCorreosPendientesService> logger,
            IConsultarListaRepository consultarListaRepository,
            IActualizarRepository actualizarRepository,
            ISmtpService smtpService)
        {
            this._logger = logger;
            this._consultarListaRepository = consultarListaRepository ?? throw new ArgumentNullException(nameof(consultarListaRepository));
            this._actualizarRepository = actualizarRepository ?? throw new ArgumentNullException(nameof(actualizarRepository));
            this._smtpService = smtpService ?? throw new ArgumentNullException(nameof(smtpService));
            this._maxIntentos = int.TryParse(Environment.GetEnvironmentVariable(ENV_MAX_INTENTOS), out int max) ? max : MAX_INTENTOS_DEFECTO;
        }

        /// <summary>
        /// Consulta los correos pendientes e intenta enviarlos por SMTP.
        /// Actualiza el estado de cada correo según el resultado del envío.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        public async Task ProcesarPendientesAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                int paginaActual = PAGINA_INICIAL;
                int totalPaginas = TOTAL_PAGINAS_INICIAL;
                do
                {
                    var resultado = await this._consultarListaRepository.ConsultarListaAsync<CorreoPendienteEntity>(
                        traceId, paginaActual,
                        c => !c.Enviado && c.Intentos < this._maxIntentos);
                    totalPaginas = resultado.CantidadPaginas;
                    paginaActual++;
                    foreach (var correo in resultado.Lista)
                        await this.ProcesarCorreoAsync(traceId, correo);
                }
                while (paginaActual <= totalPaginas);
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
        /// Intenta enviar un correo individual y actualiza su estado en la base de datos.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="correo">Entidad del correo a procesar.</param>
        public async Task ProcesarCorreoAsync(string traceId, CorreoPendienteEntity correo)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                await this._smtpService.EnviarAsync(traceId, correo.Destinatario, correo.Asunto, correo.Cuerpo, correo.EsCuerpoHtml);
                correo.Enviado = ENVIADO_EXITOSO;
                correo.FechaEdicion = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                correo.Intentos++;
                correo.UltimoError = ex.Message;
                correo.FechaEdicion = DateTime.UtcNow;
            }
            finally
            {
                await this._actualizarRepository.ActualizarAsync<CorreoPendienteEntity>(traceId, correo);
                this._logger.Fin(traceId, nombreMetodo);
            }
        }
    }
}
