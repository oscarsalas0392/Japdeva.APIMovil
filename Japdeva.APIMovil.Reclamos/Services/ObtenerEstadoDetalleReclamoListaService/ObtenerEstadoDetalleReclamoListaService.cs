using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoCacheService;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerEstadoDetalleReclamoListaService
{
    /// <summary>
    /// Servicio que resuelve el estado detalle actual de una lista de reclamos.
    /// Obtiene el detalle más reciente de la BD para cada reclamo y asigna
    /// el identificador y descripción del estado detalle desde la caché.
    /// </summary>
    public class ObtenerEstadoDetalleReclamoListaService : IObtenerEstadoDetalleReclamoListaService
    {
        private readonly ILogger<ObtenerEstadoDetalleReclamoListaService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEstadoDetalleReclamoCacheService _estadoDetalleReclamoCacheService;

        private const int PAGINA_INICIAL = 1;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerEstadoDetalleReclamoListaService"/>.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias.</param>
        /// <param name="estadoDetalleReclamoCacheService">Servicio de caché de estados detalle de reclamo.</param>
        public ObtenerEstadoDetalleReclamoListaService(
            ILogger<ObtenerEstadoDetalleReclamoListaService> logger,
            IServiceProvider serviceProvider,
            IEstadoDetalleReclamoCacheService estadoDetalleReclamoCacheService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._estadoDetalleReclamoCacheService = estadoDetalleReclamoCacheService;
        }

        /// <summary>
        /// Obtiene y asigna el estado detalle actual a cada reclamo de la lista,
        /// consultando el detalle más reciente de la BD y resolviendo la descripción desde caché.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="listaRespuesta">Lista de modelos de reclamo a enriquecer.</param>
        /// <returns>La lista con el estado detalle resuelto para cada reclamo.</returns>
        public async Task<List<ReclamoRespuestaModel>> ObtenerEstadoDetalleReclamoAsync(string traceId, List<ReclamoRespuestaModel> listaRespuesta)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                using var scope = this._serviceProvider.CreateScope();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();

                List<long> idsReclamos = listaRespuesta.Select(r => r.Id).ToList();

                var detalles = await consultarListaRepository.ConsultarListaAsync<DetalleReclamoEntity>(
                    traceId, PAGINA_INICIAL, d => idsReclamos.Contains(d.IdReclamo));

                Dictionary<long, DetalleReclamoEntity> ultimoDetallePorReclamo = detalles.Lista
                    .GroupBy(d => d.IdReclamo)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(d => d.FechaRegistro).First());

                foreach (ReclamoRespuestaModel respuesta in listaRespuesta)
                {
                    if (!ultimoDetallePorReclamo.TryGetValue(respuesta.Id, out DetalleReclamoEntity? detalle)) continue;

                    var estadoDetalle = this._estadoDetalleReclamoCacheService.ObtenerEstadoDetalleReclamo(traceId, detalle.IdEstadoDetalleReclamo);
                    if (estadoDetalle is null) continue;

                    respuesta.IdEstadoDetalleReclamo = estadoDetalle.Id;
                    respuesta.DescripcionEstadoDetalleReclamo = estadoDetalle.Descripcion;
                }

                return listaRespuesta;
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
