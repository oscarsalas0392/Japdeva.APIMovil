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
        private readonly IEnvioHistoricoDocumentoInternoService _envioHistoricoDocumentoInternoService;
        private readonly IConsultarListaRepository _consultarListaRepository;
        private readonly IEliminarRepository _eliminarRepository;
        private readonly IAgregarRepository _agregarRepository;

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
            IConsultarListaRepository consultarListaRepository,
            IEliminarRepository eliminarRepository,
            IAgregarRepository agregarRepository,
            IEnvioHistoricoDocumentoInternoService envioHistoricoDocumentoInternoService)
        {
            this._logger = logger;
            this._envioHistoricoDocumentoInternoService = envioHistoricoDocumentoInternoService;
            this._consultarListaRepository = consultarListaRepository;
            this._eliminarRepository = eliminarRepository;
            this._agregarRepository = agregarRepository;
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
                int paginaActual = PAGINA_ACTUAL_INICIAL;
                int totalPaginas = TOTAL_PAGINAS_INICIAL;

                List<DetalleReclamoEntity> listaDetalleReclamo = new List<DetalleReclamoEntity>();
                List<DetalleReclamoHistoricoEntity> listaDetalleReclamoHistorico = new List<DetalleReclamoHistoricoEntity>();
                do
                {
                    var detallesReclamos = await this._consultarListaRepository.ConsultarListaAsync<DetalleReclamoEntity>(traceId, paginaActual, x => x.IdReclamo == idReclamo);
                    listaDetalleReclamo.AddRange(detallesReclamos.Lista);
                    totalPaginas = detallesReclamos.CantidadPaginas;
                    paginaActual++;
                }
                while (paginaActual <= totalPaginas);

                if (listaDetalleReclamo.Any())
                {
                    foreach (var detalleReclamo in listaDetalleReclamo)
                    {
                        DetalleReclamoHistoricoEntity detalleReclamoEntity = new DetalleReclamoHistoricoEntity();
                        detalleReclamoEntity.Id = detalleReclamo.Id;
                        detalleReclamoEntity.Descripcion = detalleReclamo.Descripcion;
                        detalleReclamoEntity.FechaEdicion = detalleReclamo.FechaEdicion is not null ? detalleReclamo.FechaEdicion.Value.ToUniversalTime() : null;
                        detalleReclamoEntity.IdReclamo = detalleReclamo.IdReclamo;
                        detalleReclamoEntity.FechaRegistro = detalleReclamo.FechaRegistro.ToUniversalTime();
                        detalleReclamoEntity.IdEstadoDetalleReclamo = detalleReclamo.IdEstadoDetalleReclamo;
                        detalleReclamoEntity.IdDepartamento = detalleReclamo.IdDepartamento;
                        detalleReclamoEntity.IdNivelProceso = detalleReclamo.IdNivelProceso;
                        detalleReclamoEntity.IdUsuarioInterno = detalleReclamo.IdUsuarioInterno;
                        listaDetalleReclamoHistorico.Add(detalleReclamoEntity);
                    }
         
                    await this._agregarRepository.AgregarVariosAsync<DetalleReclamoHistoricoEntity>(traceId, listaDetalleReclamoHistorico);
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
