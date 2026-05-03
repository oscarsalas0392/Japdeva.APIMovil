using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Common.Repositories.EliminarRepository;
using Japdeva.APIMovil.Common.Repositories.GeneralRepository;
using Japdeva.APIMovil.EnvioCorreos.Entities;

namespace Japdeva.APIMovil.EnvioCorreos.Services.EnviarCorreoHistoricoService
{
    /// <summary>
    /// Servicio que mueve al histórico los correos enviados o con intentos agotados.
    /// El número máximo de intentos se obtiene de la variable de entorno CORREO_MAX_INTENTOS.
    /// </summary>
    public class EnviarCorreoHistoricoService : IEnviarCorreoHistoricoService
    {
        private readonly ILogger<EnviarCorreoHistoricoService> _logger;
        private readonly IConsultarListaRepository _consultarListaRepository;
        private readonly IAgregarRepository _agregarRepository;
        private readonly IEliminarRepository _eliminarRepository;
        private readonly IGeneralRepository _generalRepository;
        private readonly int _maxIntentos;
        private const string ENV_MAX_INTENTOS = "CORREO_MAX_INTENTOS";
        private const int MAX_INTENTOS_DEFECTO = 3;
        private const int PAGINA_INICIAL = 1;
        private const int TOTAL_PAGINAS_INICIAL = 0;

        /// <summary>
        /// Inicializa una nueva instancia de EnviarCorreoHistoricoService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="consultarListaRepository">Repositorio para consultar listas de entidades.</param>
        /// <param name="agregarRepository">Repositorio para agregar entidades.</param>
        /// <param name="eliminarRepository">Repositorio para eliminar entidades.</param>
        /// <param name="generalRepository">Repositorio para manejo de transacciones.</param>
        public EnviarCorreoHistoricoService(ILogger<EnviarCorreoHistoricoService> logger,
            IConsultarListaRepository consultarListaRepository,
            IAgregarRepository agregarRepository,
            IEliminarRepository eliminarRepository,
            IGeneralRepository generalRepository)
        {
            this._logger = logger;
            this._consultarListaRepository = consultarListaRepository ?? throw new ArgumentNullException(nameof(consultarListaRepository));
            this._agregarRepository = agregarRepository ?? throw new ArgumentNullException(nameof(agregarRepository));
            this._eliminarRepository = eliminarRepository ?? throw new ArgumentNullException(nameof(eliminarRepository));
            this._generalRepository = generalRepository ?? throw new ArgumentNullException(nameof(generalRepository));
            this._maxIntentos = int.TryParse(Environment.GetEnvironmentVariable(ENV_MAX_INTENTOS), out int max) ? max : MAX_INTENTOS_DEFECTO;
        }

        /// <summary>
        /// Mueve al histórico los correos enviados exitosamente o con intentos agotados,
        /// y los elimina de la tabla de pendientes.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        public async Task EnviarHistoricoAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            var transaccion = await this._generalRepository.ObtenerTransaccionBaseDatosAsync(traceId);
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                int paginaActual = PAGINA_INICIAL;
                int totalPaginas = TOTAL_PAGINAS_INICIAL;
                var listaPendientes = new List<CorreoPendienteEntity>();
                do
                {
                    var resultado = await this._consultarListaRepository.ConsultarListaAsync<CorreoPendienteEntity>(
                        traceId, paginaActual,
                        c => c.Enviado || c.Intentos >= this._maxIntentos);
                    totalPaginas = resultado.CantidadPaginas;
                    paginaActual++;
                    listaPendientes.AddRange(resultado.Lista);
                }
                while (paginaActual <= totalPaginas);

                if (listaPendientes.Any())
                {
                    var listaHistorico = listaPendientes.Select(c => this.MapearAHistorico(traceId, c)).ToList();
                    await this._agregarRepository.AgregarVariosAsync<CorreoHistoricoEntity>(traceId, listaHistorico);
                    foreach (var pendiente in listaPendientes)
                        await this._eliminarRepository.EliminarAsync<CorreoPendienteEntity>(traceId, pendiente);
                    await this._generalRepository.RealizarCommitBaseDatosAsync(traceId, transaccion);
                }
            }
            catch (Exception ex)
            {
                this._logger.Error(traceId, nombreMetodo, ex);
                if (transaccion is not null)
                    await this._generalRepository.RealizarDevolucionCambiosBaseDatosAsync(traceId, transaccion);
                throw;
            }
            finally
            {
                if (transaccion is not null)
                    await this._generalRepository.LimpiarTransaccionAsync(traceId, transaccion);
                this._logger.Fin(traceId, nombreMetodo);
            }
        }

        /// <summary>
        /// Convierte una entidad de correo pendiente en una entidad de histórico.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="pendiente">Entidad de correo pendiente a convertir.</param>
        /// <returns>Entidad de histórico con los datos del correo.</returns>
        public CorreoHistoricoEntity MapearAHistorico(string traceId, CorreoPendienteEntity pendiente)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                var historico = new CorreoHistoricoEntity();
                historico.Id = pendiente.Id;
                historico.Destinatario = pendiente.Destinatario;
                historico.Asunto = pendiente.Asunto;
                historico.Cuerpo = pendiente.Cuerpo;
                historico.Intentos = pendiente.Intentos;
                historico.UltimoError = pendiente.UltimoError;
                historico.Enviado = pendiente.Enviado;
                historico.FechaRegistro = pendiente.FechaRegistro;
                historico.FechaEdicion = pendiente.FechaEdicion;
                historico.FechaMovimiento = DateTime.UtcNow;
                return historico;
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
