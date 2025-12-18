using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.AgregarRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;


namespace Japdeva.APIMovil.Reclamos.Services.AgregarDocumentoInternoService
{
    /// <summary>
    /// Servicio para agregar documentos internos asociados a un detalle de reclamo.
    /// </summary>
    public class AgregarDocumentoInternoService : IAgregarDocumentoInternoService
    {
        private readonly ILogger<AgregarDocumentoInternoService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const string MENSAJE_ERROR_LISTA_NULA = "La lista de archivos no puede ser nula o vacía.";
        private const string MENSAJE_ERROR_DETALLE_RECLAMO_NO_ENCONTRADO = "No se encontró el detalle de reclamo con Id {0}.";
        private const bool DOCUMENTO_INTERNO_ACTIVO = true;


        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="AgregarDocumentoInternoService"/>.
        /// </summary>
        public AgregarDocumentoInternoService(
            ILogger<AgregarDocumentoInternoService> logger,
            IServiceProvider serviceProvider) 
            => (this._logger, this._serviceProvider) = (logger, serviceProvider);

        /// <summary>
        /// Agrega documentos internos asociados a un detalle de reclamo.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="agregarDocumentoInternoSolicitudModel">Modelo con los datos de la solicitud para agregar documentos internos.</param>
        /// <returns>Un <see cref="IActionResult"/> que indica el resultado de la operación.</returns>
  
        public async Task<IActionResult> AgregarDocumentoInternoAsync(string traceId, AgregarDocumentoInternoSolicitudModel agregarDocumentoInternoSolicitudModel)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var agregarRepository = scope.ServiceProvider.GetRequiredService<IAgregarRepository>();
                var detalleReclamo = await consultarRepository.ConsultarAsync<DetalleReclamoEntity>(traceId, x=> x.Id == agregarDocumentoInternoSolicitudModel.IdDetalleReclamo);
                if(detalleReclamo is null) throw new ArgumentException(string.Format(MENSAJE_ERROR_DETALLE_RECLAMO_NO_ENCONTRADO, agregarDocumentoInternoSolicitudModel.IdDetalleReclamo));      
                
                if (agregarDocumentoInternoSolicitudModel.ListaDocumentos is null || !agregarDocumentoInternoSolicitudModel.ListaDocumentos.Any()) throw new ArgumentException(MENSAJE_ERROR_LISTA_NULA);

                List<DocumentoInternoEntity> listaDocumentoInterno = new List<DocumentoInternoEntity>();

                foreach (var archivo in agregarDocumentoInternoSolicitudModel.ListaDocumentos)
                {
                    if (string.IsNullOrEmpty(archivo.NombreArchivo) || string.IsNullOrEmpty(archivo.ContenidoArchivo)) continue;
                    DocumentoInternoEntity documentoInternoEntity = new DocumentoInternoEntity();
                    documentoInternoEntity.Activo = DOCUMENTO_INTERNO_ACTIVO;
                    documentoInternoEntity.Documento = archivo.ContenidoArchivo;
                    documentoInternoEntity.FechaRegistro = DateTime.Now;
                    documentoInternoEntity.IdDetalleReclamo = detalleReclamo.Id;
                    documentoInternoEntity.NombreDocumento = archivo.NombreArchivo;
                    listaDocumentoInterno.Add(documentoInternoEntity);
                }

                if(!listaDocumentoInterno.Any()) throw new ArgumentException(MENSAJE_ERROR_LISTA_NULA);

                await agregarRepository.AgregarVariosAsync<DocumentoInternoEntity>(traceId, listaDocumentoInterno);

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
