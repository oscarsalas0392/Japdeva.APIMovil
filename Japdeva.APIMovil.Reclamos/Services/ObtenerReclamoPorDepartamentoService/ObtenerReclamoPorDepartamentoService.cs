using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;


namespace Japdeva.APIMovil.Reclamos.Services.ObtenerReclamoPorDepartamentoService
{
    /// <summary>
    /// Servicio para obtener reclamos por departamento.
    /// </summary>
    public class ObtenerReclamoPorDepartamentoService: IObtenerReclamoPorDepartamentoService
    {
        private readonly ILogger<ObtenerReclamoPorDepartamentoService> _logger;
        private readonly IConsultarListaRepository _consultarListaRepository;

        private const string MENSAJE_ERROR_ID_PAGINA = "La pagina es inválida, debe ser mayor a 0.";
        private const int ID_PAGINA_MINIMO = 1;
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerReclamoPorDepartamentoService"/>.
        /// </summary>
        /// <param name="logger">Instancia de logger para la clase.</param>
        /// <param name="consultarListaRepository">Repositorio para consultar listas.</param>
        public ObtenerReclamoPorDepartamentoService(
            ILogger<ObtenerReclamoPorDepartamentoService> logger,
            IConsultarListaRepository consultarListaRepository)
        {
            _logger = logger;
            _consultarListaRepository = consultarListaRepository;
        }

        /// <summary>
        /// Obtiene los reclamos asociados a un departamento específico de forma paginada.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idDepartamento">Identificador del departamento para filtrar los reclamos.</param>
        /// <param name="pagina">Número de página para la paginación de resultados.</param>
        /// <returns>Una acción HTTP que contiene la lista de reclamos encontrados.</returns>
        public async Task<IActionResult> ObtenerReclamosPorDepartamentoAsync(string traceId, int idDepartamento, int pagina)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                if (pagina < ID_PAGINA_MINIMO) throw new ArgumentException(MENSAJE_ERROR_ID_PAGINA);

                var reclamos = await this._consultarListaRepository.ConsultarListaAsync<ReclamoEntity>(traceId, pagina,
                    reclamo => reclamo.IdDepartamentoActual == idDepartamento);

                var respuesta = new RespuestaListaModel<ReclamoRespuestaModel>();
                respuesta.TotalRegistros = reclamos.TotalRegistros;
                respuesta.CantidadPaginas = reclamos.CantidadPaginas;
                respuesta.PaginaActual = reclamos.PaginaActual;
                respuesta.Lista = reclamos.Lista.Select(r => new ReclamoRespuestaModel()
                {
                    Id = r.Id,
                    Titulo = r.Titulo,
                    Descripcion = r.Descripcion,
                    IdEstadoReclamo = r.IdEstadoReclamo,
                    IdUsuarioExterno = r.IdUsuarioExterno,
                    FechaRegistro = r.FechaRegistro,
                    IdDepartamentoActual = r.IdDepartamentoActual,
                    DescripcionDepartamento = r.DescripcionDepartamentoActual
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
