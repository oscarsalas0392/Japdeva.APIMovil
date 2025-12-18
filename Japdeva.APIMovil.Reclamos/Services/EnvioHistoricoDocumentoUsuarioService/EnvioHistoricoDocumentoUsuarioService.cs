using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Common.Repositories.EliminarRepository;
using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.EnvioHistoricoDocumentoUsuarioService
{
    /// <summary>
    /// Servicio para enviar los documentos de usuario asociados a un reclamo al histórico,
    /// eliminando los originales y agregando los históricos.
    /// </summary>
    public class EnvioHistoricoDocumentoUsuarioService : IEnvioHistoricoDocumentoUsuarioService
    {
        private readonly ILogger<EnvioHistoricoDocumentoUsuarioService> _logger;
        private readonly IServiceProvider _serviceProvider;

        private const int PAGINA_ACTUAL_INICIAL = 1;
        private const int TOTAL_PAGINAS_INICIAL = 0;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="EnvioHistoricoDocumentoUsuarioService"/>.
        /// </summary>
        /// <param name="logger">Instancia del registrador de logs.</param>
        /// <param name="consultarListaRepository">Repositorio para consultar listas.</param>
        /// <param name="eliminarRepository">Repositorio para eliminar entidades.</param>
        /// <param name="agregarRepository">Repositorio para agregar entidades.</param>
        public EnvioHistoricoDocumentoUsuarioService(
            ILogger<EnvioHistoricoDocumentoUsuarioService> logger,
            IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;

        }

        /// <summary>
        /// Envía los documentos de usuario asociados a un reclamo al histórico, eliminando los originales y agregando los históricos.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el registro de logs.</param>
        /// <param name="idReclamo">Identificador del reclamo cuyos documentos serán procesados.</param>
        public async Task EnviarHistoricoDocumentoUsuarioAsync(string traceId, long idReclamo)
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
                
                List<DocumentoUsuarioEntity> listaDocumentosUsuario = new List<DocumentoUsuarioEntity>();
                List<DocumentoUsuarioHistoricoEntity> listaDocumentosUsuarioHistorico = new List<DocumentoUsuarioHistoricoEntity>();
                do 
                {
                   var documentosUsuarios = await consultarListaRepository.ConsultarListaAsync<DocumentoUsuarioEntity>(traceId, paginaActual, x => x.IdReclamo == idReclamo);
                   listaDocumentosUsuario.AddRange(documentosUsuarios.Lista);

                    foreach (var documentoUsuario in documentosUsuarios.Lista)
                    {
                        DocumentoUsuarioHistoricoEntity documentoUsuarioHistoricoEntity = new DocumentoUsuarioHistoricoEntity();
                        documentoUsuarioHistoricoEntity.Id = documentoUsuario.Id;
                        documentoUsuarioHistoricoEntity.IdReclamo = documentoUsuario.IdReclamo;
                        documentoUsuarioHistoricoEntity.NombreDocumento = documentoUsuario.NombreDocumento;
                        documentoUsuarioHistoricoEntity.Documento = documentoUsuario.Documento;
                        documentoUsuarioHistoricoEntity.FechaRegistro = documentoUsuario.FechaRegistro;
                        listaDocumentosUsuarioHistorico.Add(documentoUsuarioHistoricoEntity);
                    }
                    totalPaginas = documentosUsuarios.CantidadPaginas;
                }
                while (paginaActual <= totalPaginas);

                await eliminarRepository.EliminarVariosAsync<DocumentoUsuarioEntity>(traceId, listaDocumentosUsuario);
                await agregarRepository.AgregarVariosAsync<DocumentoUsuarioHistoricoEntity>(traceId, listaDocumentosUsuarioHistorico);

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
