using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoCacheService;


namespace Japdeva.APIMovil.Reclamos.Services.ObtenerDetalleReclamoService
{
    /// <summary>
    /// Servicio para obtener el detalle de un reclamo. Enruta la consulta a la tabla activa o
    /// histórica según el valor de <c>EstaEnHistorico</c> en la entidad del reclamo.
    /// </summary>
    public class ObtenerDetalleReclamoService : IObtenerDetalleReclamoService
    {
        private readonly ILogger<ObtenerDetalleReclamoService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEstadoDetalleReclamoCacheService _estadoDetalleReclamoCacheService;

        private const string MENSAJE_ERROR_ESTADO_NO_ENCONTRADO = "El estado detalle de reclamo con Id {0} no fue encontrado en caché.";
        private const string MENSAJE_ERROR_RECLAMO_NO_ENCONTRADO = "El reclamo con Id {0} no fue encontrado.";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerDetalleReclamoService"/>.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias.</param>
        /// <param name="estadoDetalleReclamoCacheService">Servicio de caché de estados detalle de reclamo.</param>
        public ObtenerDetalleReclamoService(
            ILogger<ObtenerDetalleReclamoService> logger,
            IServiceProvider serviceProvider,
            IEstadoDetalleReclamoCacheService estadoDetalleReclamoCacheService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._estadoDetalleReclamoCacheService = estadoDetalleReclamoCacheService;
        }

        /// <summary>
        /// Obtiene el detalle de un reclamo específico. Si el reclamo está en histórico,
        /// consulta <c>Tbl_DetalleReclamoHistorico</c>; de lo contrario, consulta <c>Tbl_DetalleReclamo</c>.
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

                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();

                var reclamo = await consultarRepository.ConsultarAsync<ReclamoEntity>(traceId, x => x.Id == idReclamo);
                if (reclamo is null) throw new KeyNotFoundException(string.Format(MENSAJE_ERROR_RECLAMO_NO_ENCONTRADO, idReclamo));

                RespuestaListaModel<DetalleReclamoRespuestaModel> respuesta = new RespuestaListaModel<DetalleReclamoRespuestaModel>();
                List<DetalleReclamoRespuestaModel> listaDetalleReclamo = new List<DetalleReclamoRespuestaModel>();

                if (reclamo.EstaEnHistorico)
                {
                    var detallesHistorico = await consultarListaRepository.ConsultarListaAsync<DetalleReclamoHistoricoEntity>(traceId, pagina, x => x.IdReclamo == idReclamo);

                    foreach (var entity in detallesHistorico.Lista)
                    {
                        var estadoDetalle = this._estadoDetalleReclamoCacheService.ObtenerEstadoDetalleReclamo(traceId, entity.IdEstadoDetalleReclamo);
                        if (estadoDetalle is null) throw new KeyNotFoundException(string.Format(MENSAJE_ERROR_ESTADO_NO_ENCONTRADO, entity.IdEstadoDetalleReclamo));

                        DetalleReclamoRespuestaModel detalleRespuesta = new DetalleReclamoRespuestaModel();
                        detalleRespuesta.Id = entity.Id;
                        detalleRespuesta.IdReclamo = entity.IdReclamo;
                        detalleRespuesta.IdUsuarioInterno = entity.IdUsuarioInterno;
                        detalleRespuesta.NombreUsuarioInterno = string.Empty;
                        detalleRespuesta.IdNivelProceso = entity.IdNivelProceso;
                        detalleRespuesta.IdDepartamento = entity.IdDepartamento;
                        detalleRespuesta.NombreDepartamento = string.Empty;
                        detalleRespuesta.IdEstadoDetalleReclamo = entity.IdEstadoDetalleReclamo;
                        detalleRespuesta.DescripcionEstadoDetalleReclamo = estadoDetalle.Descripcion;
                        detalleRespuesta.Descripcion = entity.Descripcion;
                        listaDetalleReclamo.Add(detalleRespuesta);
                    }

                    respuesta.PaginaActual = detallesHistorico.PaginaActual;
                    respuesta.CantidadPaginas = detallesHistorico.CantidadPaginas;
                    respuesta.TotalRegistros = detallesHistorico.TotalRegistros;
                }
                else
                {
                    var detallesActivos = await consultarListaRepository.ConsultarListaAsync<DetalleReclamoEntity>(traceId, pagina, x => x.IdReclamo == idReclamo);

                    foreach (var entity in detallesActivos.Lista)
                    {
                        var estadoDetalle = this._estadoDetalleReclamoCacheService.ObtenerEstadoDetalleReclamo(traceId, entity.IdEstadoDetalleReclamo);
                        if (estadoDetalle is null) throw new KeyNotFoundException(string.Format(MENSAJE_ERROR_ESTADO_NO_ENCONTRADO, entity.IdEstadoDetalleReclamo));

                        DetalleReclamoRespuestaModel detalleRespuesta = new DetalleReclamoRespuestaModel();
                        detalleRespuesta.Id = entity.Id;
                        detalleRespuesta.IdReclamo = entity.IdReclamo;
                        detalleRespuesta.IdUsuarioInterno = entity.IdUsuarioInterno;
                        detalleRespuesta.NombreUsuarioInterno = string.Empty;
                        detalleRespuesta.IdNivelProceso = entity.IdNivelProceso;
                        detalleRespuesta.IdDepartamento = entity.IdDepartamento;
                        detalleRespuesta.NombreDepartamento = string.Empty;
                        detalleRespuesta.IdEstadoDetalleReclamo = entity.IdEstadoDetalleReclamo;
                        detalleRespuesta.DescripcionEstadoDetalleReclamo = estadoDetalle.Descripcion;
                        detalleRespuesta.Descripcion = entity.Descripcion;
                        listaDetalleReclamo.Add(detalleRespuesta);
                    }

                    respuesta.PaginaActual = detallesActivos.PaginaActual;
                    respuesta.CantidadPaginas = detallesActivos.CantidadPaginas;
                    respuesta.TotalRegistros = detallesActivos.TotalRegistros;
                }

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
