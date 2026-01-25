using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Common.Repositories.EliminarRepository;
using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.EnvioHistoricoDocumentoInternoService
{
    /// <summary>
    /// Servicio encargado de enviar los documentos internos asociados a un detalle de reclamo al histórico,
    /// eliminando los documentos actuales y agregando sus copias en la tabla histórica.
    /// </summary>
    public class EnvioHistoricoDocumentoInternoService : IEnvioHistoricoDocumentoInternoService
    {
        private readonly ILogger<EnvioHistoricoDocumentoInternoService> _logger;
        private readonly IConsultarListaRepository _consultarListaRepository;
        private readonly IAgregarRepository _agregarRepository;
        private readonly IEliminarRepository _eliminarRepository;

        private const int PAGINA_ACTUAL_INICIAL = 1;
        private const int TOTAL_PAGINAS_INICIAL = 0;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="EnvioHistoricoDocumentoInternoService"/>.
        /// </summary>
        /// <param name="logger">Instancia del registrador de logs.</param>
        /// <param name="serviceProvider">Proveedor de servicios para la obtención de dependencias.</param>
        public EnvioHistoricoDocumentoInternoService(
            ILogger<EnvioHistoricoDocumentoInternoService> logger,
            IConsultarListaRepository consultarListaRepository,
            IAgregarRepository agregarRepository,
            IEliminarRepository eliminarRepository
            )
        {
            this._logger = logger;
            this._consultarListaRepository = consultarListaRepository;
            this._agregarRepository = agregarRepository;
            this._eliminarRepository = eliminarRepository;

        }

        /// <summary>
        /// Envía los documentos internos asociados a un detalle de reclamo al histórico, eliminando los documentos actuales y agregando sus copias en la tabla histórica.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idReclamo">Identificador del reclamo cuyos documentos serán procesados.</param>
        public async Task EnviarHistoricoDocumentoInternoAsync(string traceId, long idReclamo)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                int paginaActual = PAGINA_ACTUAL_INICIAL;
                int totalPaginas = TOTAL_PAGINAS_INICIAL;

                var respuestaDetalleReclamo = await this._consultarListaRepository.ConsultarListaAsync<DetalleReclamoEntity>(traceId, paginaActual, x => x.IdReclamo == idReclamo);

                foreach (var detalleReclamo in respuestaDetalleReclamo.Lista)
                {
                    paginaActual = PAGINA_ACTUAL_INICIAL;
                    List<DocumentoInternoEntity> listaDocumentosInterno = new List<DocumentoInternoEntity>();
                    List<DocumentoInternoHistoricoEntity> listaDocumentosInternoHistorico = new List<DocumentoInternoHistoricoEntity>();
                    do
                    {
                        var documentosUsuarios = await this._consultarListaRepository.ConsultarListaAsync<DocumentoInternoEntity>(traceId, paginaActual, x => x.IdDetalleReclamo == detalleReclamo.Id);
                        listaDocumentosInterno.AddRange(documentosUsuarios.Lista);
                        totalPaginas = documentosUsuarios.CantidadPaginas;
                        paginaActual++;
                    }
                    while (paginaActual <= totalPaginas);

                    foreach (var documentoInterno in listaDocumentosInterno)
                    {
                        DocumentoInternoHistoricoEntity documentoUsuarioHistoricoEntity = new DocumentoInternoHistoricoEntity();
                        documentoUsuarioHistoricoEntity.Id = documentoInterno.Id;
                        documentoUsuarioHistoricoEntity.IdDetalleReclamo = documentoInterno.IdDetalleReclamo;
                        documentoUsuarioHistoricoEntity.NombreDocumento = documentoInterno.NombreDocumento;
                        documentoUsuarioHistoricoEntity.Documento = documentoInterno.Documento;
                        documentoUsuarioHistoricoEntity.FechaRegistro = documentoInterno.FechaRegistro;
                        documentoUsuarioHistoricoEntity.IdUsuarioInterno = documentoInterno.IdUsuarioInterno;
                        documentoUsuarioHistoricoEntity.Activo = documentoInterno.Activo;
                        listaDocumentosInternoHistorico.Add(documentoUsuarioHistoricoEntity);
                    }

                    if (listaDocumentosInterno.Any())
                    {
                        await this._eliminarRepository.EliminarVariosAsync<DocumentoInternoEntity>(traceId, listaDocumentosInterno);
                        await this._agregarRepository.AgregarVariosAsync<DocumentoInternoHistoricoEntity>(traceId, listaDocumentosInternoHistorico);
                    }
                     
                   if(respuestaDetalleReclamo.Lista.Any()) 
                        await this._eliminarRepository.EliminarVariosAsync<DetalleReclamoEntity>(traceId, respuestaDetalleReclamo.Lista);
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
