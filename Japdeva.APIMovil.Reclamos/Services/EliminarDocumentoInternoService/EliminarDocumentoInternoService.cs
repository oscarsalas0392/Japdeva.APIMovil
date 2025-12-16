using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Common.Repositories.EliminarRepository;
using Japdeva.APIMovil.Reclamos.Entities;

namespace Japdeva.APIMovil.Reclamos.Services.EliminarDocumentoInternoService
{
    /// <summary>
    /// Servicio para eliminar documentos internos asociados a reclamos.
    /// Proporciona funcionalidades para eliminar documentos internos del sistema
    /// con validaciones previas y manejo de errores robusto.
    /// </summary>
    public class EliminarDocumentoInternoService : IEliminarDocumentoInternoService
    {
        private readonly ILogger<EliminarDocumentoInternoService> _logger;
        private readonly IConsultarRepository _consultarRepository;
        private readonly IEliminarRepository _eliminarRepository;

        private const string MENSAJE_DOCUMENTO_INTERNO_NO_EXISTE = "El documento interno con el id {0} no existe";

        /// <summary>
        /// Inicializa una nueva instancia del servicio de eliminación de documentos internos.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos y errores.</param>
        /// <param name="consultarRepository">Repositorio para operaciones de consulta.</param>
        /// <param name="eliminarRepository">Repositorio para operaciones de eliminación.</param>
        public EliminarDocumentoInternoService(
            ILogger<EliminarDocumentoInternoService> logger,
            IConsultarRepository consultarRepository,
            IEliminarRepository eliminarRepository
            )
        {
            this._logger = logger;
            this._consultarRepository = consultarRepository;
            this._eliminarRepository = eliminarRepository;
        }

        /// <summary>
        /// Elimina un documento interno del sistema de forma asíncrona.
        /// Valida la existencia del documento antes de eliminarlo para garantizar la integridad de datos.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idDocumento">Identificador único del documento interno a eliminar.</param>
        /// <returns>Resultado de la operación de eliminación del documento interno.</returns>
        public async Task<IActionResult> EliminarDocumentoInternoAsync(string traceId, long idDocumento) 
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                
                var documentoInterno = await this._consultarRepository.ConsultarAsync<DocumentoInternoEntity>(traceId, x => x.Id == idDocumento);
                if(documentoInterno is null)  
                    throw new ArgumentException(string.Format(MENSAJE_DOCUMENTO_INTERNO_NO_EXISTE, idDocumento));
                
                await this._eliminarRepository.EliminarAsync<DocumentoInternoEntity>(traceId, documentoInterno);
                
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
    }
}