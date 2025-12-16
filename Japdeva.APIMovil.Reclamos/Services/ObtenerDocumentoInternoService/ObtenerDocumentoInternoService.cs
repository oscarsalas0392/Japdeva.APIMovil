using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;


namespace Japdeva.APIMovil.Reclamos.Services.ObtenerDocumentoInternoService
{
    /// <summary>
    /// Servicio para obtener documentos internos asociados a un detalle de reclamo.
    /// </summary>
    public class ObtenerDocumentoInternoService: IObtenerDocumentoInternoService
    {
        private readonly ILogger<ObtenerDocumentoInternoService> _logger;
        private readonly IConsultarListaRepository _consultarListaRepository;
        private const int PAGINACION_INICIO = 1;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerDocumentoInternoService"/>.
        /// </summary>
        /// <param name="logger">Instancia del registrador para el servicio.</param>
        /// <param name="consultarListaRepository">Repositorio para consultar listas de documentos internos.</param>
        public ObtenerDocumentoInternoService(
            ILogger<ObtenerDocumentoInternoService> logger,
            IConsultarListaRepository consultarListaRepository)          
        {
            this._logger = logger;
            this._consultarListaRepository = consultarListaRepository;
        }

        /// <summary>
        /// Obtiene de forma asíncrona los documentos internos asociados a un detalle de reclamo específico.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idDetalleReclamo">Identificador del detalle de reclamo para el cual se consultan los documentos internos.</param>
        /// <returns>Una acción de resultado que contiene la lista de documentos internos encontrados.</returns>
        public async Task<IActionResult> ObtenerDocumentosInternosAsync(string traceId, long idDetalleReclamo)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                RespuestaListaModel<DocumentoInternoRespuestaModel> respuesta = new RespuestaListaModel<DocumentoInternoRespuestaModel>();
                var documentosInternos = await this._consultarListaRepository.ConsultarListaAsync<DocumentoInternoEntity>(traceId, PAGINACION_INICIO, x=>x.IdDetalleReclamo == idDetalleReclamo);

                respuesta.CantidadPaginas = documentosInternos.CantidadPaginas;
                respuesta.PaginaActual = documentosInternos.PaginaActual;
                respuesta.TotalRegistros = documentosInternos.TotalRegistros;
                respuesta.Lista = documentosInternos.Lista.Select(doc => new DocumentoInternoRespuestaModel
                {
                    Id = doc.Id,
                    IdDetalleReclamo = doc.IdDetalleReclamo,
                    NombreDocumento = doc.NombreDocumento,
                    Documento = doc.Documento
                }).ToList();

                return new OkObjectResult(respuesta);
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
