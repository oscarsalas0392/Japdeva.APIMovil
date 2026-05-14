using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoCacheService;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerDetalleReclamoPorDepartamentoEstadoService
{
    /// <summary>
    /// Servicio para obtener el último detalle de reclamo filtrado por departamento, estado detalle y reclamo.
    /// </summary>
    public class ObtenerDetalleReclamoPorDepartamentoEstadoService : IObtenerDetalleReclamoPorDepartamentoEstadoService
    {
        private readonly ILogger<ObtenerDetalleReclamoPorDepartamentoEstadoService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEstadoDetalleReclamoCacheService _estadoDetalleReclamoCacheService;

        private const string MENSAJE_ERROR_ESTADO_NO_ENCONTRADO = "El estado detalle de reclamo con Id {0} no fue encontrado.";
        private const string MENSAJE_ERROR_DETALLE_NO_ENCONTRADO = "No se encontró un detalle de reclamo para los filtros indicados.";
        private const int PAGINA_INICIAL = 1;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerDetalleReclamoPorDepartamentoEstadoService"/>.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias.</param>
        /// <param name="estadoDetalleReclamoCacheService">Servicio de caché de estados detalle de reclamo.</param>
        public ObtenerDetalleReclamoPorDepartamentoEstadoService(
            ILogger<ObtenerDetalleReclamoPorDepartamentoEstadoService> logger,
            IServiceProvider serviceProvider,
            IEstadoDetalleReclamoCacheService estadoDetalleReclamoCacheService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._estadoDetalleReclamoCacheService = estadoDetalleReclamoCacheService;
        }

        /// <summary>
        /// Obtiene el último detalle de reclamo que pertenece al departamento, estado detalle y reclamo indicados.
        /// Selecciona el registro con la fecha de registro más reciente.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idDepartamento">Identificador del departamento a filtrar.</param>
        /// <param name="idEstadoDetalleReclamo">Identificador del estado detalle de reclamo a filtrar.</param>
        /// <param name="idReclamo">Identificador del reclamo a filtrar.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        public async Task<IActionResult> ObtenerDetalleReclamoPorDepartamentoEstadoAsync(string traceId, long idDepartamento, int idEstadoDetalleReclamo, long idReclamo)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                var estadoDetalle = this._estadoDetalleReclamoCacheService.ObtenerEstadoDetalleReclamo(traceId, idEstadoDetalleReclamo);
                if (estadoDetalle is null) throw new KeyNotFoundException(string.Format(MENSAJE_ERROR_ESTADO_NO_ENCONTRADO, idEstadoDetalleReclamo));

                using var scope = this._serviceProvider.CreateScope();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();

                var detalles = await consultarListaRepository.ConsultarListaAsync<DetalleReclamoEntity>(traceId, PAGINA_INICIAL,
                    x => x.IdDepartamento == idDepartamento && x.IdEstadoDetalleReclamo == idEstadoDetalleReclamo && x.IdReclamo == idReclamo);

                DetalleReclamoEntity? ultimo = detalles.Lista.OrderByDescending(x => x.FechaRegistro).FirstOrDefault();
                if (ultimo is null) throw new KeyNotFoundException(MENSAJE_ERROR_DETALLE_NO_ENCONTRADO);

                DetalleReclamoRespuestaModel respuesta = new DetalleReclamoRespuestaModel
                {
                    Id = ultimo.Id,
                    IdReclamo = ultimo.IdReclamo,
                    IdUsuarioInterno = ultimo.IdUsuarioInterno,
                    NombreUsuarioInterno = string.Empty,
                    IdNivelProceso = ultimo.IdNivelProceso,
                    IdDepartamento = ultimo.IdDepartamento,
                    NombreDepartamento = string.Empty,
                    IdEstadoDetalleReclamo = ultimo.IdEstadoDetalleReclamo,
                    DescripcionEstadoDetalleReclamo = estadoDetalle.Descripcion,
                    Descripcion = ultimo.Descripcion
                };

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
