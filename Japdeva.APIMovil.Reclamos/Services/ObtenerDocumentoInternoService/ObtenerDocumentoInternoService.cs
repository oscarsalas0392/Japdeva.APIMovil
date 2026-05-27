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
    /// Busca primero en la tabla activa y, si no hay resultados, busca en la tabla histórica.
    /// </summary>
    public class ObtenerDocumentoInternoService : IObtenerDocumentoInternoService
    {
        private readonly ILogger<ObtenerDocumentoInternoService> _logger;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerDocumentoInternoService"/>.
        /// </summary>
        /// <param name="logger">Instancia del registrador para el servicio.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias.</param>
        public ObtenerDocumentoInternoService(
            ILogger<ObtenerDocumentoInternoService> logger,
            IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Obtiene los documentos internos de un detalle de reclamo. Consulta primero
        /// <c>Tbl_DocumentoInterno</c> y, si no hay resultados, consulta <c>Tbl_DocumentoInternoHistorico</c>.
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

                var documentosActivos = await consultarListaRepository.ConsultarListaAsync<DocumentoInternoEntity>(traceId, pagina, x => x.IdDetalleReclamo == idDetalleReclamo);

                if (documentosActivos.Lista.Any())
                {
                    RespuestaListaModel<DocumentoInternoRespuestaModel> respuestaActiva = new RespuestaListaModel<DocumentoInternoRespuestaModel>();
                    respuestaActiva.CantidadPaginas = documentosActivos.CantidadPaginas;
                    respuestaActiva.PaginaActual = documentosActivos.PaginaActual;
                    respuestaActiva.TotalRegistros = documentosActivos.TotalRegistros;
                    respuestaActiva.Lista = documentosActivos.Lista.Select(doc => new DocumentoInternoRespuestaModel
                    {
                        Id = doc.Id,
                        IdDetalleReclamo = doc.IdDetalleReclamo,
                        NombreDocumento = doc.NombreDocumento,
                        Documento = doc.Documento
                    }).ToList();
                    return new OkObjectResult(respuestaActiva);
                }

                var documentosHistorico = await consultarListaRepository.ConsultarListaAsync<DocumentoInternoHistoricoEntity>(traceId, pagina, x => x.IdDetalleReclamo == idDetalleReclamo);

                RespuestaListaModel<DocumentoInternoRespuestaModel> respuestaHistorica = new RespuestaListaModel<DocumentoInternoRespuestaModel>();
                respuestaHistorica.CantidadPaginas = documentosHistorico.CantidadPaginas;
                respuestaHistorica.PaginaActual = documentosHistorico.PaginaActual;
                respuestaHistorica.TotalRegistros = documentosHistorico.TotalRegistros;
                respuestaHistorica.Lista = documentosHistorico.Lista.Select(doc => new DocumentoInternoRespuestaModel
                {
                    Id = doc.Id,
                    IdDetalleReclamo = doc.IdDetalleReclamo,
                    NombreDocumento = doc.NombreDocumento,
                    Documento = doc.Documento
                }).ToList();
                return new OkObjectResult(respuestaHistorica);
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
