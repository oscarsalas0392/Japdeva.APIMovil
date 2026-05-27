using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.ListaRespuestaReclamoService;
using Japdeva.APIMovil.Reclamos.Services.ObtenerEstadoDetalleReclamoListaService;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerReclamoPorIdService
{
    /// <summary>
    /// Servicio para obtener un reclamo específico por su identificador único,
    /// incluyendo el estado detalle actual y el usuario interno asignado.
    /// </summary>
    public class ObtenerReclamoPorIdService : IObtenerReclamoPorIdService
    {
        private readonly ILogger<ObtenerReclamoPorIdService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IListaRespuestaReclamoService _listaRespuestaReclamoService;
        private readonly IObtenerEstadoDetalleReclamoListaService _obtenerEstadoDetalleReclamoListaService;

        private const string MENSAJE_ERROR_RECLAMO_NO_ENCONTRADO = "El reclamo con Id {0} no fue encontrado.";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerReclamoPorIdService"/>.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias.</param>
        /// <param name="listaRespuestaReclamoService">Servicio para construir el modelo de respuesta del reclamo.</param>
        /// <param name="obtenerEstadoDetalleReclamoListaService">Servicio para resolver el estado detalle y usuario asignado.</param>
        public ObtenerReclamoPorIdService(
            ILogger<ObtenerReclamoPorIdService> logger,
            IServiceProvider serviceProvider,
            IListaRespuestaReclamoService listaRespuestaReclamoService,
            IObtenerEstadoDetalleReclamoListaService obtenerEstadoDetalleReclamoListaService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._listaRespuestaReclamoService = listaRespuestaReclamoService;
            this._obtenerEstadoDetalleReclamoListaService = obtenerEstadoDetalleReclamoListaService;
        }

        /// <summary>
        /// Obtiene un reclamo específico por su identificador único, enriquecido con el estado
        /// detalle actual y el nombre del usuario interno asignado.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idReclamo">Identificador único del reclamo a consultar.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        public async Task<IActionResult> ObtenerReclamoPorIdAsync(string traceId, long idReclamo)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                using var scope = this._serviceProvider.CreateScope();
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();

                ReclamoEntity? reclamoEntity = await consultarRepository.ConsultarAsync<ReclamoEntity>(traceId, x => x.Id == idReclamo);
                if (reclamoEntity is null) throw new KeyNotFoundException(string.Format(MENSAJE_ERROR_RECLAMO_NO_ENCONTRADO, idReclamo));

                List<ReclamoEntity> lista = [reclamoEntity];
                List<ReclamoRespuestaModel> listaRespuesta = await this._listaRespuestaReclamoService.ObtenerListaRespuestaReclamoAsync(traceId, lista);
                listaRespuesta = await this._obtenerEstadoDetalleReclamoListaService.ObtenerEstadoDetalleReclamoAsync(traceId, listaRespuesta);

                return new OkObjectResult(listaRespuesta.First());
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
