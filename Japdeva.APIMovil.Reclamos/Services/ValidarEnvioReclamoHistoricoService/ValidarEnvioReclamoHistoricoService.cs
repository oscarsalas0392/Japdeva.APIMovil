using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Common.Repositories.GeneralRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EnvioHistoricoDetalleReclamoService;
using Japdeva.APIMovil.Reclamos.Services.EnvioHistoricoDocumentoUsuarioService;

namespace Japdeva.APIMovil.Reclamos.Services.ValidarEnvioReclamoHistoricoService
{
    /// <summary>
    /// Servicio para validar y procesar el envío de reclamos al histórico del sistema.
    /// Se encarga de identificar reclamos que cumplan criterios específicos de tiempo y estado
    /// para ser archivados en las tablas históricas correspondientes.
    /// </summary>
    public class ValidarEnvioReclamoHistoricoService : IValidarEnvioReclamoHistoricoService
    {
        private readonly ILogger<ValidarEnvioReclamoHistoricoService> _logger;
        private readonly IConsultarListaRepository _consultarListaRepository;
        private readonly IEnvioHistoricoDetalleReclamoService _envioHistoricoDetalleReclamoService;
        private readonly IEnvioHistoricoDocumentoUsuarioService _envioHistoricoDocumentoUsuarioService;
        private readonly IGeneralRepository _generalRepository;

        private readonly string _mesesRestar = Environment.GetEnvironmentVariable(MESES_ENVIO_HISTORICO) ?? MESES_ENVIO_HISTORICO_DEFECTO;
        private const int PAGINA_INICIAL = 1;
        private const string MESES_ENVIO_HISTORICO = "VARIABLE_ENTORNO_MESES_ENVIO_HISTORICO";
        private const string MESES_ENVIO_HISTORICO_DEFECTO = "-3";

        /// <summary>
        /// Inicializa una nueva instancia del servicio de validación de envío histórico de reclamos.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos y errores.</param>
        /// <param name="consultarListaRepository">Repositorio para consultas paginadas de listas.</param>
        /// <param name="envioHistoricoDetalleReclamoService">Servicio para envío de detalles de reclamo al histórico.</param>
        /// <param name="envioHistoricoDocumentoUsuarioService">Servicio para envío de documentos de usuario al histórico.</param>
        /// <param name="generalRepository">Repositorio para operaciones generales y transacciones.</param>
        public ValidarEnvioReclamoHistoricoService(
            ILogger<ValidarEnvioReclamoHistoricoService> logger,
            IConsultarListaRepository consultarListaRepository,
            IEnvioHistoricoDetalleReclamoService envioHistoricoDetalleReclamoService,
            IEnvioHistoricoDocumentoUsuarioService envioHistoricoDocumentoUsuarioService,
            IGeneralRepository generalRepository)
        {
            this._logger = logger;
            this._consultarListaRepository = consultarListaRepository;
            this._envioHistoricoDetalleReclamoService = envioHistoricoDetalleReclamoService;
            this._envioHistoricoDocumentoUsuarioService = envioHistoricoDocumentoUsuarioService;
            this._generalRepository = generalRepository;
        }

        /// <summary>
        /// Valida y realiza el envío histórico de los reclamos según los criterios definidos.
        /// Procesa reclamos que cumplan condiciones de tiempo y estado para archivarlos en tablas históricas.
        /// Utiliza configuración de variables de entorno para determinar el período de retención.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <exception cref="ArgumentException">Se lanza cuando la variable de entorno de meses no es un número válido.</exception>
        /// <exception cref="InvalidOperationException">Se lanza cuando ocurre un error durante el procesamiento de reclamos históricos.</exception>
        public async Task ValidarEnvioReclamoHistoricoAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            var transaccion = await this._generalRepository.ObtenerTransaccionBaseDatosAsync(traceId);
            try 
            {
                this._logger.Inicio(traceId, nombreMetodo);
                
                // Validar y convertir configuración de meses
                if (!int.TryParse(this._mesesRestar, out int mesesARestar))
                {
                    throw new ArgumentException($"La variable de entorno {MESES_ENVIO_HISTORICO} debe ser un número válido. Valor actual: {this._mesesRestar}");
                }

                DateTime fechaCorte = DateTime.Now.AddMonths(mesesARestar);

                // Obtener reclamos elegibles para envío histórico
                var reclamos = await this._consultarListaRepository.ConsultarListaAsync<ReclamoEntity>(traceId, PAGINA_INICIAL,
                    x => x.FechaRegistro <= fechaCorte
                    && x.IdEstadoReclamo != (int)EstadoReclamoModel.EnProceso
                    && x.IdEstadoReclamo != (int)EstadoReclamoModel.Pendiente);

                // Procesar cada reclamo de forma paralela
                foreach(var reclamo in reclamos.Lista)
                {
                    Task tareaEnvioHistoricoDocumentoUsuario = this._envioHistoricoDocumentoUsuarioService.EnviarHistoricoDocumentoUsuarioAsync(traceId, reclamo.Id);
                    Task tareaDetalleReclamo = this._envioHistoricoDetalleReclamoService.EnviarHistoricoDetalleReclamoAsync(traceId, reclamo.Id);
                    
                    await Task.WhenAll(tareaEnvioHistoricoDocumentoUsuario, tareaDetalleReclamo);
                }

                await this._generalRepository.RealizarCommitBaseDatosAsync(traceId, transaccion);
            }
            catch(Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                if (transaccion is not null) 
                    await this._generalRepository.RealizarDevolucionCambiosBaseDatosAsync(traceId, transaccion);
                throw;
            }
            finally
            {
                if(transaccion is not null)  
                    await this._generalRepository.LimpiarTransaccionAsync(traceId, transaccion);
                this._logger.Fin(traceId, nombreMetodo);
            }
        }
    }
}
