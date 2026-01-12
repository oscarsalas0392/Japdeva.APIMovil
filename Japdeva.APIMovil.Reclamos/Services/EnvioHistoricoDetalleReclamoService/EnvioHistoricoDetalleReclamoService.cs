using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Common.Repositories.EliminarRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Services.EnvioHistoricoDocumentoInternoService;

namespace Japdeva.APIMovil.Reclamos.Services.EnvioHistoricoDetalleReclamoService
{
    /// <summary>
    /// Servicio encargado de enviar los detalles de un reclamo al histórico, eliminando los registros actuales y agregando los registros históricos correspondientes.
    /// </summary>
    public class EnvioHistoricoDetalleReclamoService : IEnvioHistoricoDetalleReclamoService
    {
        private readonly ILogger<EnvioHistoricoDetalleReclamoService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnvioHistoricoDocumentoInternoService _envioHistoricoDocumentoInternoService;

        private const int PAGINA_ACTUAL_INICIAL = 1;
        private const int TOTAL_PAGINAS_INICIAL = 0;

 
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="EnvioHistoricoDetalleReclamoService"/>.
        /// </summary>
        /// <param name="logger">Instancia del logger para registrar información de la operación.</param>
        /// <param name="consultarListaRepository">Repositorio para consultar listas de entidades.</param>
        /// <param name="serviceProvider">Proveedor de servicios para la obtención de dependencias.</param>
        /// <param name="envioHistoricoDocumentoInternoService">Servicio para el envío de documentos internos al histórico.</param>
        public EnvioHistoricoDetalleReclamoService(
            ILogger<EnvioHistoricoDetalleReclamoService> logger,
            IServiceProvider serviceProvider,
            IEnvioHistoricoDocumentoInternoService envioHistoricoDocumentoInternoService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._envioHistoricoDocumentoInternoService = envioHistoricoDocumentoInternoService;
        }

        /// <summary>
        /// Envía los detalles del reclamo especificado al histórico, eliminando los registros actuales y agregando los registros históricos correspondientes.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idReclamo">Identificador del reclamo cuyos detalles serán procesados.</param>
        public async Task EnviarHistoricoDetalleReclamoAsync(string traceId, long idReclamo)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                var eliminarRepository = scope.ServiceProvider.GetRequiredService<IEliminarRepository>();
                var agregarRepository = scope.ServiceProvider.GetRequiredService<IAgregarRepository>();
                int paginaActual = PAGINA_ACTUAL_INICIAL;
                int totalPaginas = TOTAL_PAGINAS_INICIAL;

                List<DetalleReclamoEntity> listaDetalleReclamo = new List<DetalleReclamoEntity>();
                List<DetalleReclamoHistoricoEntity> listaDetalleReclamoHistorico = new List<DetalleReclamoHistoricoEntity>();
                do
                {
                    var detallesReclamos = await consultarListaRepository.ConsultarListaAsync<DetalleReclamoEntity>(traceId, paginaActual, x => x.IdReclamo == idReclamo);
                    listaDetalleReclamo.AddRange(detallesReclamos.Lista);
                    totalPaginas = detallesReclamos.CantidadPaginas;
                }
                while (paginaActual <= totalPaginas);

                if (!listaDetalleReclamo.Any())
                {
                    foreach (var detalleReclamo in listaDetalleReclamo)
                    {
                        DetalleReclamoHistoricoEntity detalleReclamoEntity = new DetalleReclamoHistoricoEntity();
                        detalleReclamoEntity.Id = detalleReclamo.Id;
                        detalleReclamoEntity.Descripcion = detalleReclamo.Descripcion;
                        detalleReclamoEntity.FechaEdicion = detalleReclamo.FechaEdicion;
                        detalleReclamoEntity.IdReclamo = detalleReclamo.IdReclamo;
                        detalleReclamoEntity.FechaRegistro = detalleReclamo.FechaRegistro;
                        detalleReclamoEntity.IdEstadoDetalleReclamo = detalleReclamo.IdEstadoDetalleReclamo;
                        detalleReclamoEntity.IdDepartamento = detalleReclamo.IdDepartamento;
                        detalleReclamoEntity.IdNivelProceso = detalleReclamo.IdNivelProceso;
                        detalleReclamoEntity.IdUsuarioInterno = detalleReclamo.IdUsuarioInterno;
                        listaDetalleReclamoHistorico.Add(detalleReclamoEntity);
                        await this._envioHistoricoDocumentoInternoService.EnviarHistoricoDocumentoInternoAsync(traceId, detalleReclamo.Id);
                    }

                    Task tareaEliminar = eliminarRepository.EliminarVariosAsync<DetalleReclamoEntity>(traceId, listaDetalleReclamo);
                    Task tareaAgregarHistorico = agregarRepository.AgregarVariosAsync<DetalleReclamoHistoricoEntity>(traceId, listaDetalleReclamoHistorico);
                    await Task.WhenAll(tareaEliminar, tareaAgregarHistorico);
                }              
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
