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
    /// Busca primero en la tabla activa y, si no encuentra el registro, busca en la tabla histórica.
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
        /// Obtiene un detalle de reclamo por su identificador único. Busca primero en
        /// <c>Tbl_DetalleReclamo</c> y si no lo encuentra, lo busca en <c>Tbl_DetalleReclamoHistorico</c>.
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

                var detalleActivo = await consultarRepository.ConsultarAsync<DetalleReclamoEntity>(traceId, x => x.Id == idDetalleReclamo);

                if (detalleActivo is not null)
                {
                    var estadoDetalleActivo = this._estadoDetalleReclamoCacheService.ObtenerEstadoDetalleReclamo(traceId, detalleActivo.IdEstadoDetalleReclamo);
                    if (estadoDetalleActivo is null) throw new KeyNotFoundException(string.Format(MENSAJE_ERROR_ESTADO_NO_ENCONTRADO, detalleActivo.IdEstadoDetalleReclamo));

                    DetalleReclamoRespuestaModel respuestaActiva = new DetalleReclamoRespuestaModel
                    {
                        Id = detalleActivo.Id,
                        IdReclamo = detalleActivo.IdReclamo,
                        IdUsuarioInterno = detalleActivo.IdUsuarioInterno,
                        NombreUsuarioInterno = string.Empty,
                        IdNivelProceso = detalleActivo.IdNivelProceso,
                        IdDepartamento = detalleActivo.IdDepartamento,
                        NombreDepartamento = string.Empty,
                        IdEstadoDetalleReclamo = detalleActivo.IdEstadoDetalleReclamo,
                        DescripcionEstadoDetalleReclamo = estadoDetalleActivo.Descripcion,
                        Descripcion = detalleActivo.Descripcion
                    };
                    return new OkObjectResult(respuestaActiva);
                }

                var detalleHistorico = await consultarRepository.ConsultarAsync<DetalleReclamoHistoricoEntity>(traceId, x => x.Id == idDetalleReclamo);
                if (detalleHistorico is null) throw new KeyNotFoundException(string.Format(MENSAJE_ERROR_DETALLE_NO_ENCONTRADO, idDetalleReclamo));

                var estadoDetalleHistorico = this._estadoDetalleReclamoCacheService.ObtenerEstadoDetalleReclamo(traceId, detalleHistorico.IdEstadoDetalleReclamo);
                if (estadoDetalleHistorico is null) throw new KeyNotFoundException(string.Format(MENSAJE_ERROR_ESTADO_NO_ENCONTRADO, detalleHistorico.IdEstadoDetalleReclamo));

                DetalleReclamoRespuestaModel respuestaHistorica = new DetalleReclamoRespuestaModel
                {
                    Id = detalleHistorico.Id,
                    IdReclamo = detalleHistorico.IdReclamo,
                    IdUsuarioInterno = detalleHistorico.IdUsuarioInterno,
                    NombreUsuarioInterno = string.Empty,
                    IdNivelProceso = detalleHistorico.IdNivelProceso,
                    IdDepartamento = detalleHistorico.IdDepartamento,
                    NombreDepartamento = string.Empty,
                    IdEstadoDetalleReclamo = detalleHistorico.IdEstadoDetalleReclamo,
                    DescripcionEstadoDetalleReclamo = estadoDetalleHistorico.Descripcion,
                    Descripcion = detalleHistorico.Descripcion
                };
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
