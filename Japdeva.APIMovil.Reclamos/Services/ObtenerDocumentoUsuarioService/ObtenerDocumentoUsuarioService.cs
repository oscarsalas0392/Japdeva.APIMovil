using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;


namespace Japdeva.APIMovil.Reclamos.Services.ObtenerDocumentoUsuarioService
{
    /// <summary>
    /// Servicio para obtener documentos de usuario asociados a reclamos específicos.
    /// Proporciona funcionalidades para consultar y recuperar documentos externos
    /// agregados por usuarios durante el proceso de reclamos.
    /// </summary>
    public class ObtenerDocumentoUsuarioService : IObtenerDocumentoUsuarioService
    {
        private readonly ILogger<ObtenerDocumentoUsuarioService> _logger;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Inicializa una nueva instancia del servicio de obtención de documentos de usuario.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos y errores.</param>
        /// <param name="consultarListaRepository">Repositorio para consultas paginadas de listas.</param>
        public ObtenerDocumentoUsuarioService(ILogger<ObtenerDocumentoUsuarioService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Obtiene los documentos de usuario asociados a un reclamo específico de forma paginada.
        /// Recupera todos los documentos externos proporcionados por usuarios para un reclamo determinado.
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
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();

                var documentosUsuarios = await consultarListaRepository.ConsultarListaAsync<DocumentoUsuarioEntity>(traceId, pagina, x => x.IdReclamo == idReclamo);

                var respuesta = new RespuestaListaModel<DocumentoUsuarioRespuestaModel>();
                respuesta.TotalRegistros = documentosUsuarios.TotalRegistros;
                respuesta.CantidadPaginas = documentosUsuarios.CantidadPaginas;
                respuesta.PaginaActual = documentosUsuarios.PaginaActual;

                if (documentosUsuarios is not null && documentosUsuarios.Lista.Any())
                {
                    respuesta.Lista = documentosUsuarios.Lista.Select(x => new DocumentoUsuarioRespuestaModel()
                    {
                        Id = x.Id,
                        IdReclamo = x.IdReclamo,
                        NombreDocumento = x.NombreDocumento,
                        Documento = x.Documento
                    }).ToList();
                }
                else
                {
                    respuesta.Lista = new List<DocumentoUsuarioRespuestaModel>();
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
