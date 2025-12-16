using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoCacheService;


namespace Japdeva.APIMovil.Reclamos.Services.ObtenerDetalleReclamoService
{
    /// <summary>
    /// Servicio para obtener el detalle de un reclamo, incluyendo información de usuarios internos,
    /// departamentos asociados y la descripción del estado del detalle del reclamo.
    /// </summary>
    public class ObtenerDetalleReclamoService : IObtenerDetalleReclamoService
    {
        private readonly ILogger<ObtenerDetalleReclamoService> _logger;
        private readonly IConsultarListaRepository _consultarListaRepository;
        private readonly IEstadoDetalleReclamoCacheService _estadoDetalleReclamoCacheService;

        private const string MENSAJE_ERROR_DETALLE_RECLAMO_NO_ENCONTRADO = "El detalle de reclamo con Id {0} no fue encontrado.";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerDetalleReclamoService"/>.
        /// </summary>
        public ObtenerDetalleReclamoService(
            ILogger<ObtenerDetalleReclamoService> logger,
            IConsultarListaRepository consultarListaRepository,
            IEstadoDetalleReclamoCacheService estadoDetalleReclamoCacheService)
        {
            this._logger = logger;
            this._consultarListaRepository = consultarListaRepository;
            this._estadoDetalleReclamoCacheService = estadoDetalleReclamoCacheService;
        }

        /// <summary>
        /// Obtiene el detalle de un reclamo específico, incluyendo información de usuarios internos y departamentos asociados,
        /// así como la descripción del estado del detalle del reclamo. El resultado es paginado.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idReclamo">Identificador único del reclamo a consultar.</param>
        /// <param name="pagina">Número de página para la paginación de resultados.</param>
        /// <returns>Un <see cref="IActionResult"/> que contiene la respuesta con el detalle del reclamo.</returns>
        public async Task<IActionResult> ObtenerDetalleReclamoAsync(string traceId, long idReclamo, int pagina)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                RespuestaListaModel<DetalleReclamoRespuestaModel> respuesta = new RespuestaListaModel<DetalleReclamoRespuestaModel>();
                List<DetalleReclamoRespuestaModel> listaDetalleReclamo = new List<DetalleReclamoRespuestaModel>();
                var detalleReclamos = await this._consultarListaRepository.ConsultarListaAsync<DetalleReclamoEntity>(traceId, pagina, x => x.IdReclamo == idReclamo);


                List<long> listaIdUsuarioInterno = detalleReclamos.Lista.Where(x => x.IdUsuarioInterno is not null)
                                                   .Select(x => x.IdUsuarioInterno!.Value)
                                                   .Distinct()
                                                   .ToList();

                List<long> listaIdDepartamento = detalleReclamos.Lista
                                   .Select(x => x.IdDepartamento)
                                   .Distinct()
                                   .ToList();

                //AQUI FALTA IR A MICROSERVICIO DE USUARIOS PARA OBTENER NOMBRES DE USUARIOS INTERNOS
                //AQUI FALTA IR A MICROSERVICIO DE USUARIOS PARA OBTENER NOMBRES DE DEPARTAMENTOS

                foreach (var detalleReclamoEntity in respuesta.Lista)
                {
                    DetalleReclamoRespuestaModel detalleReclamo = new DetalleReclamoRespuestaModel();

                    var estadoDetalleDescripcion = this._estadoDetalleReclamoCacheService.ObtenerEstadoDetalleReclamo(traceId, detalleReclamoEntity.IdEstadoDetalleReclamo);
                    if (estadoDetalleDescripcion is null) throw new Exception(string.Format(MENSAJE_ERROR_DETALLE_RECLAMO_NO_ENCONTRADO, detalleReclamoEntity.IdEstadoDetalleReclamo));

                    detalleReclamo.Id = detalleReclamoEntity.Id;
                    detalleReclamo.IdReclamo = detalleReclamoEntity.IdReclamo;
                    detalleReclamo.IdUsuarioInterno = detalleReclamoEntity.IdUsuarioInterno;
                    detalleReclamo.NombreUsuarioInterno = "";
                    detalleReclamo.IdNivelProceso = detalleReclamoEntity.IdNivelProceso;
                    detalleReclamo.IdDepartamento = detalleReclamoEntity.IdDepartamento;
                    detalleReclamo.NombreDepartamento = "";
                    detalleReclamo.IdEstadoDetalleReclamo = detalleReclamoEntity.IdEstadoDetalleReclamo;
                    detalleReclamo.DescripcionEstadoDetalleReclamo = estadoDetalleDescripcion.Descripcion;
                    detalleReclamo.Descripcion = detalleReclamoEntity.Descripcion;

                    listaDetalleReclamo.Add(detalleReclamo);
                }

                respuesta.PaginaActual = detalleReclamos.PaginaActual;
                respuesta.CantidadPaginas = detalleReclamos.CantidadPaginas;
                respuesta.TotalRegistros = detalleReclamos.TotalRegistros;
                respuesta.Lista = listaDetalleReclamo;

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
