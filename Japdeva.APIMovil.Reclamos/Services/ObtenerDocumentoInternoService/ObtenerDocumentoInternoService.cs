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
        private readonly IServiceProvider _serviceProvider;


        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerDocumentoInternoService"/>.
        /// </summary>
        /// <param name="logger">Instancia del registrador para el servicio.</param>
        /// <param name="consultarListaRepository">Repositorio para consultar listas de documentos internos.</param>
        public ObtenerDocumentoInternoService(
            ILogger<ObtenerDocumentoInternoService> logger,
            IServiceProvider serviceProvider)          
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Obtiene de forma asíncrona los documentos internos asociados a un detalle de reclamo específico.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idDetalleReclamo">Identificador del detalle de reclamo para el cual se consultan los documentos internos.</param>
        /// <param name="pagina">Número de página para la paginación de los resultados.</param>
        /// <returns>Una acción de resultado que contiene la lista de documentos internos encontrados.</returns>
        public async Task<IActionResult> ObtenerDocumentosInternosAsync(string traceId, long idDetalleReclamo, int pagina)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                using var scope = this._serviceProvider.CreateScope();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                RespuestaListaModel<DocumentoInternoRespuestaModel> respuesta = new RespuestaListaModel<DocumentoInternoRespuestaModel>();
                var documentosInternos = await consultarListaRepository.ConsultarListaAsync<DocumentoInternoEntity>(traceId, pagina, x=>x.IdDetalleReclamo == idDetalleReclamo);

                if (documentosInternos is not null && documentosInternos.Lista.Any())
                {
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
                }

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
