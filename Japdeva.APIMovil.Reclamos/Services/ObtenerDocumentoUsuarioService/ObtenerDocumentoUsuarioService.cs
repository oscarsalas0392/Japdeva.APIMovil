using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;


namespace Japdeva.APIMovil.Reclamos.Services.ObtenerDocumentoUsuarioService
{
    /// <summary>
    /// Servicio para obtener documentos de usuario asociados a reclamos específicos.
    /// Enruta la consulta a la tabla activa o histórica según el valor de <c>EstaEnHistorico</c>.
    /// </summary>
    public class ObtenerDocumentoUsuarioService : IObtenerDocumentoUsuarioService
    {
        private readonly ILogger<ObtenerDocumentoUsuarioService> _logger;
        private readonly IServiceProvider _serviceProvider;

        private const string MENSAJE_ERROR_RECLAMO_NO_ENCONTRADO = "El reclamo con Id {0} no fue encontrado.";

        /// <summary>
        /// Inicializa una nueva instancia del servicio de obtención de documentos de usuario.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos y errores.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias.</param>
        public ObtenerDocumentoUsuarioService(ILogger<ObtenerDocumentoUsuarioService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Obtiene los documentos de usuario de un reclamo. Si el reclamo está en histórico,
        /// consulta <c>Tbl_DocumentoUsuarioHistorico</c>; de lo contrario, consulta <c>Tbl_DocumentoUsuario</c>.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idReclamo">Identificador del reclamo del cual obtener los documentos.</param>
        /// <param name="pagina">Número de página para la consulta paginada.</param>
        /// <returns>Resultado de la operación con la lista paginada de documentos de usuario.</returns>
        public async Task<IActionResult> ObtenerDocumentoUsuarioAsync(string traceId, long idReclamo, int pagina)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();

                var reclamo = await consultarRepository.ConsultarAsync<ReclamoEntity>(traceId, x => x.Id == idReclamo);
                if (reclamo is null) throw new KeyNotFoundException(string.Format(MENSAJE_ERROR_RECLAMO_NO_ENCONTRADO, idReclamo));

                var respuesta = new RespuestaListaModel<DocumentoUsuarioRespuestaModel>();

                if (reclamo.EstaEnHistorico)
                {
                    var documentosHistorico = await consultarListaRepository.ConsultarListaAsync<DocumentoUsuarioHistoricoEntity>(traceId, pagina, x => x.IdReclamo == idReclamo);
                    respuesta.TotalRegistros = documentosHistorico.TotalRegistros;
                    respuesta.CantidadPaginas = documentosHistorico.CantidadPaginas;
                    respuesta.PaginaActual = documentosHistorico.PaginaActual;
                    respuesta.Lista = documentosHistorico.Lista.Select(x => new DocumentoUsuarioRespuestaModel
                    {
                        Id = x.Id,
                        IdReclamo = x.IdReclamo,
                        NombreDocumento = x.NombreDocumento,
                        Documento = x.Documento
                    }).ToList();
                }
                else
                {
                    var documentosActivos = await consultarListaRepository.ConsultarListaAsync<DocumentoUsuarioEntity>(traceId, pagina, x => x.IdReclamo == idReclamo);
                    respuesta.TotalRegistros = documentosActivos.TotalRegistros;
                    respuesta.CantidadPaginas = documentosActivos.CantidadPaginas;
                    respuesta.PaginaActual = documentosActivos.PaginaActual;
                    respuesta.Lista = documentosActivos.Lista.Select(x => new DocumentoUsuarioRespuestaModel
                    {
                        Id = x.Id,
                        IdReclamo = x.IdReclamo,
                        NombreDocumento = x.NombreDocumento,
                        Documento = x.Documento
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
