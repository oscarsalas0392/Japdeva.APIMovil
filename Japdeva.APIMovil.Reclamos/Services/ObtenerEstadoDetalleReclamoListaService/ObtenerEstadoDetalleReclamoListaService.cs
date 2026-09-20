using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EstadoDetalleReclamoCacheService;
using Japdeva.APIMovil.Reclamos.Services.UsuarioInternoNombreCacheService;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerEstadoDetalleReclamoListaService
{
    /// <summary>
    /// Servicio que resuelve el estado detalle actual y el usuario asignado de una lista de reclamos.
    /// Obtiene el detalle más reciente de la BD para cada reclamo y asigna
    /// el identificador y descripción del estado detalle desde la caché.
    /// También resuelve el nombre del usuario interno asignado desde la caché de usuarios.
    /// </summary>
    public class ObtenerEstadoDetalleReclamoListaService : IObtenerEstadoDetalleReclamoListaService
    {
        private readonly ILogger<ObtenerEstadoDetalleReclamoListaService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEstadoDetalleReclamoCacheService _estadoDetalleReclamoCacheService;
        private readonly IUsuarioInternoNombreCacheService _usuarioInternoNombreCacheService;

        private const int LISTA_VACIA = 0;
        private const int PAGINA_INICIAL = 1;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerEstadoDetalleReclamoListaService"/>.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias.</param>
        /// <param name="estadoDetalleReclamoCacheService">Servicio de caché de estados detalle de reclamo.</param>
        /// <param name="usuarioInternoNombreCacheService">Servicio de caché de nombres de usuarios internos.</param>
        public ObtenerEstadoDetalleReclamoListaService(
            ILogger<ObtenerEstadoDetalleReclamoListaService> logger,
            IServiceProvider serviceProvider,
            IEstadoDetalleReclamoCacheService estadoDetalleReclamoCacheService,
            IUsuarioInternoNombreCacheService usuarioInternoNombreCacheService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._estadoDetalleReclamoCacheService = estadoDetalleReclamoCacheService;
            this._usuarioInternoNombreCacheService = usuarioInternoNombreCacheService;
        }

        /// <summary>
        /// Obtiene y asigna el estado detalle actual y el usuario asignado a cada reclamo de la lista,
        /// consultando todos los detalles de la BD y resolviendo la descripción desde caché.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="listaRespuesta">Lista de modelos de reclamo a enriquecer.</param>
        /// <returns>La lista con el estado detalle y usuario asignado resueltos para cada reclamo.</returns>
        public async Task<List<ReclamoRespuestaModel>> ObtenerEstadoDetalleReclamoAsync(string traceId, List<ReclamoRespuestaModel> listaRespuesta)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                using var scope = this._serviceProvider.CreateScope();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();

                List<long> idsReclamos = listaRespuesta.Select(r => r.Id).ToList();

                List<DetalleReclamoEntity> todosLosDetalles = [];
                int paginaActual = PAGINA_INICIAL;
                int totalPaginas = PAGINA_INICIAL;
                do
                {
                    var pagina = await consultarListaRepository.ConsultarListaAsync<DetalleReclamoEntity>(
                        traceId, paginaActual, d => idsReclamos.Contains(d.IdReclamo));
                    todosLosDetalles.AddRange(pagina.Lista);
                    totalPaginas = pagina.CantidadPaginas;
                    paginaActual++;
                }
                while (paginaActual <= totalPaginas);

                Dictionary<long, DetalleReclamoEntity> ultimoDetallePorReclamo = todosLosDetalles
                    .GroupBy(d => d.IdReclamo)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(d => d.FechaRegistro).First());

                Dictionary<long, string> nombresPorUsuario = this.ObtenerNombresUsuarios(traceId, ultimoDetallePorReclamo.Values);

                foreach (ReclamoRespuestaModel respuesta in listaRespuesta)
                {
                    if (!ultimoDetallePorReclamo.TryGetValue(respuesta.Id, out DetalleReclamoEntity? detalle)) continue;

                    var estadoDetalle = this._estadoDetalleReclamoCacheService.ObtenerEstadoDetalleReclamo(traceId, detalle.IdEstadoDetalleReclamo);
                    if (estadoDetalle is null) continue;

                    respuesta.IdEstadoDetalleReclamo = estadoDetalle.Id;
                    respuesta.DescripcionEstadoDetalleReclamo = estadoDetalle.Descripcion;
                    respuesta.IdUsuarioInterno = detalle.IdUsuarioInterno;

                    if (detalle.IdUsuarioInterno.HasValue && nombresPorUsuario.TryGetValue(detalle.IdUsuarioInterno.Value, out string? nombre))
                        respuesta.NombreUsuarioInterno = nombre;
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

        /// <summary>
        /// Obtiene los nombres de los usuarios internos asignados a los detalles desde la caché.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="detalles">Colección de detalles de reclamo con posibles usuarios asignados.</param>
        /// <returns>Diccionario de idUsuario → nombre completo.</returns>
        public Dictionary<long, string> ObtenerNombresUsuarios(string traceId, IEnumerable<DetalleReclamoEntity> detalles)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            Dictionary<long, string> resultado = [];
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                List<long> idsUsuarios = detalles
                    .Where(d => d.IdUsuarioInterno.HasValue)
                    .Select(d => d.IdUsuarioInterno!.Value)
                    .Distinct()
                    .ToList();

                if (idsUsuarios.Count == LISTA_VACIA) return resultado;

                foreach (long idUsuario in idsUsuarios)
                {
                    string? nombre = this._usuarioInternoNombreCacheService.ObtenerNombrePorId(traceId, idUsuario);
                    if (nombre is not null)
                        resultado[idUsuario] = nombre;
                }

                return resultado;
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
