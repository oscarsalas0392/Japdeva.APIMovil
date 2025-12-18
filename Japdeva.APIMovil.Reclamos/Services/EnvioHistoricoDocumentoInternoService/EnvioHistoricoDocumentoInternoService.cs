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
        private readonly IServiceProvider _serviceProvider;

        private const int PAGINA_ACTUAL_INICIAL = 1;
        private const int TOTAL_PAGINAS_INICIAL = 0;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="EnvioHistoricoDocumentoInternoService"/>.
        /// </summary>
        /// <param name="logger">Instancia del registrador de logs.</param>
        /// <param name="serviceProvider">Proveedor de servicios para la obtención de dependencias.</param>
        public EnvioHistoricoDocumentoInternoService(
            ILogger<EnvioHistoricoDocumentoInternoService> logger,
            IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Envía los documentos internos asociados a un detalle de reclamo al histórico, eliminando los documentos actuales y agregando sus copias en la tabla histórica.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idDetalleReclamo">Identificador del detalle de reclamo cuyos documentos serán procesados.</param>
        public async Task EnviarHistoricoDocumentoInternoAsync(string traceId, long idDetalleReclamo)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var _scope = this._serviceProvider.CreateScope();
                var consultarListaRepository = _scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                var agregarRepository = _scope.ServiceProvider.GetRequiredService<IAgregarRepository>();
                var eliminarRepository = _scope.ServiceProvider.GetRequiredService<IEliminarRepository>();
                int paginaActual = PAGINA_ACTUAL_INICIAL;
                int totalPaginas = TOTAL_PAGINAS_INICIAL;

                List<DocumentoInternoEntity> listaDocumentosUsuario = new List<DocumentoInternoEntity>();
                List<DocumentoInternoHistoricoEntity> listaDocumentosUsuarioHistorico = new List<DocumentoInternoHistoricoEntity>();
                do
                {
                    var documentosUsuarios = await consultarListaRepository.ConsultarListaAsync<DocumentoInternoEntity>(traceId, paginaActual, x => x.IdDetalleReclamo == idDetalleReclamo);
                    listaDocumentosUsuario.AddRange(documentosUsuarios.Lista);
                    totalPaginas = documentosUsuarios.CantidadPaginas;
                }
                while (paginaActual <= totalPaginas);

                if (!listaDocumentosUsuario.Any())
                {
                    foreach (var documentoUsuario in listaDocumentosUsuario)
                    {
                        DocumentoInternoHistoricoEntity documentoUsuarioHistoricoEntity = new DocumentoInternoHistoricoEntity();
                        documentoUsuarioHistoricoEntity.Id = documentoUsuario.Id;
                        documentoUsuarioHistoricoEntity.IdDetalleReclamo = documentoUsuario.IdDetalleReclamo;
                        documentoUsuarioHistoricoEntity.NombreDocumento = documentoUsuario.NombreDocumento;
                        documentoUsuarioHistoricoEntity.Documento = documentoUsuario.Documento;
                        documentoUsuarioHistoricoEntity.FechaRegistro = documentoUsuario.FechaRegistro;
                        listaDocumentosUsuarioHistorico.Add(documentoUsuarioHistoricoEntity);
                    }

                    Task tareaEliminar = eliminarRepository.EliminarVariosAsync<DocumentoInternoEntity>(traceId, listaDocumentosUsuario);
                    Task tareaAgregarHistorico = agregarRepository.AgregarVariosAsync<DocumentoInternoHistoricoEntity>(traceId, listaDocumentosUsuarioHistorico);
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
