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
        private readonly IServiceProvider _serviceProvider;

        private const string MENSAJE_DOCUMENTO_INTERNO_NO_EXISTE = "El documento interno con el id {0} no existe";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="EliminarDocumentoInternoService"/>.
        /// </summary>
        /// <param name="logger">Instancia del registrador de logs.</param>
        /// <param name="serviceProvider">Proveedor de servicios para la obtención de dependencias.</param>
        public EliminarDocumentoInternoService(
            ILogger<EliminarDocumentoInternoService> logger,
            IServiceProvider serviceProvider
            )
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
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
                using var scope = this._serviceProvider.CreateAsyncScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var eliminarRepository = scope.ServiceProvider.GetRequiredService<IEliminarRepository>();

                var documentoInterno = await consultarRepository.ConsultarAsync<DocumentoInternoEntity>(traceId, x => x.Id == idDocumento);
                if(documentoInterno is null)  
                    throw new ArgumentException(string.Format(MENSAJE_DOCUMENTO_INTERNO_NO_EXISTE, idDocumento));
                
                await eliminarRepository.EliminarAsync<DocumentoInternoEntity>(traceId, documentoInterno);
                
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