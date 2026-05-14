using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoCacheService;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerDetalleReclamoPorIdDetalleService
{
    /// <summary>
    /// Servicio para obtener un detalle de reclamo específico por su identificador único.
    /// </summary>
    public class ObtenerDetalleReclamoPorIdDetalleService : IObtenerDetalleReclamoPorIdDetalleService
    {
        private readonly ILogger<ObtenerDetalleReclamoPorIdDetalleService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEstadoDetalleReclamoCacheService _estadoDetalleReclamoCacheService;

        private const string MENSAJE_ERROR_DETALLE_NO_ENCONTRADO = "El detalle de reclamo con Id {0} no fue encontrado.";
        private const string MENSAJE_ERROR_ESTADO_NO_ENCONTRADO = "El estado detalle de reclamo con Id {0} no fue encontrado.";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerDetalleReclamoPorIdDetalleService"/>.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias.</param>
        /// <param name="estadoDetalleReclamoCacheService">Servicio de caché de estados detalle de reclamo.</param>
        public ObtenerDetalleReclamoPorIdDetalleService(
            ILogger<ObtenerDetalleReclamoPorIdDetalleService> logger,
            IServiceProvider serviceProvider,
            IEstadoDetalleReclamoCacheService estadoDetalleReclamoCacheService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._estadoDetalleReclamoCacheService = estadoDetalleReclamoCacheService;
        }

        /// <summary>
        /// Obtiene un detalle de reclamo específico por su identificador único.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idDetalleReclamo">Identificador único del detalle de reclamo a consultar.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        public async Task<IActionResult> ObtenerDetalleReclamoPorIdDetalleAsync(string traceId, long idDetalleReclamo)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();

                var detalleEntity = await consultarRepository.ConsultarAsync<DetalleReclamoEntity>(traceId, x => x.Id == idDetalleReclamo);
                if (detalleEntity is null) throw new KeyNotFoundException(string.Format(MENSAJE_ERROR_DETALLE_NO_ENCONTRADO, idDetalleReclamo));

                var estadoDetalle = this._estadoDetalleReclamoCacheService.ObtenerEstadoDetalleReclamo(traceId, detalleEntity.IdEstadoDetalleReclamo);
                if (estadoDetalle is null) throw new KeyNotFoundException(string.Format(MENSAJE_ERROR_ESTADO_NO_ENCONTRADO, detalleEntity.IdEstadoDetalleReclamo));

                DetalleReclamoRespuestaModel respuesta = new DetalleReclamoRespuestaModel
                {
                    Id = detalleEntity.Id,
                    IdReclamo = detalleEntity.IdReclamo,
                    IdUsuarioInterno = detalleEntity.IdUsuarioInterno,
                    NombreUsuarioInterno = string.Empty,
                    IdNivelProceso = detalleEntity.IdNivelProceso,
                    IdDepartamento = detalleEntity.IdDepartamento,
                    NombreDepartamento = string.Empty,
                    IdEstadoDetalleReclamo = detalleEntity.IdEstadoDetalleReclamo,
                    DescripcionEstadoDetalleReclamo = estadoDetalle.Descripcion,
                    Descripcion = detalleEntity.Descripcion
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
