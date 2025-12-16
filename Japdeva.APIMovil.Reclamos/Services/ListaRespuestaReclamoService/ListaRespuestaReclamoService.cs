using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EstadoReclamoCacheService;

namespace Japdeva.APIMovil.Reclamos.Services.ListaRespuestaReclamoService
{
    /// <summary>
    /// Servicio para obtener una lista de modelos de respuesta de reclamo a partir de entidades de reclamo.
    /// </summary>
    public class ListaRespuestaReclamoService : IListaRespuestaReclamoService
    {
        private readonly ILogger<ListaRespuestaReclamoService> _logger;
        private readonly IEstadoReclamoCacheService _estadoReclamoCacheService;

        private const string MENSAJE_ERROR_ESTADO_RECLAMO_NO_EXISTE = "No se encontró el estado de reclamo en caché para el Id:{0}";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ListaRespuestaReclamoService"/>.
        /// </summary>
        public ListaRespuestaReclamoService(ILogger<ListaRespuestaReclamoService> logger,
            IEstadoReclamoCacheService estadoReclamoCacheService)
        {
            this._logger = logger;
            this._estadoReclamoCacheService = estadoReclamoCacheService;
        }

        /// <summary>
        /// Obtiene una lista de modelos de respuesta de reclamo a partir de una lista de entidades de reclamo.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el registro de logs.</param>
        /// <param name="listaReclamoEntity">Lista de entidades de reclamo a procesar.</param>
        /// <returns>Una lista de <see cref="ReclamoRespuestaModel"/> con la información de los reclamos.</returns>
        public async Task<List<ReclamoRespuestaModel>> ObtenerListaRespuestaReclamoAsync(string traceId, List<ReclamoEntity> listaReclamoEntity)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                List<ReclamoRespuestaModel> listaReclamoRespuestas = new List<ReclamoRespuestaModel>();
                foreach (var reclamoEntity in listaReclamoEntity)
                {
                    var estadoReclamoCache = this._estadoReclamoCacheService.ObtenerEstadoReclamo(traceId, reclamoEntity.IdEstadoReclamo);
                    if (estadoReclamoCache is null) throw new Exception(string.Format(MENSAJE_ERROR_ESTADO_RECLAMO_NO_EXISTE, reclamoEntity.IdEstadoReclamo));
                    ReclamoRespuestaModel reclamoRespuesta = new ReclamoRespuestaModel();
                    reclamoRespuesta.Id = reclamoEntity.Id;
                    reclamoRespuesta.Titulo = reclamoEntity.Titulo;
                    reclamoRespuesta.Descripcion = reclamoEntity.Descripcion;
                    reclamoRespuesta.IdEstadoReclamo = reclamoEntity.IdEstadoReclamo;
                    reclamoRespuesta.IdUsuarioExterno = reclamoEntity.IdUsuarioExterno;
                    reclamoRespuesta.FechaRegistro = reclamoEntity.FechaRegistro;
                    reclamoRespuesta.IdDepartamentoActual = (int)reclamoEntity.IdDepartamentoActual;
                    reclamoRespuesta.DescripcionEstadoReclamo = estadoReclamoCache.Descripcion;
                    reclamoRespuesta.DescripcionDepartamento = "";
                    reclamoRespuesta.NombreUsuarioExterno = "";
                    reclamoRespuesta.EstaEnHistorico = reclamoEntity.EstaEnHistorico;
                    listaReclamoRespuestas.Add(reclamoRespuesta);
                }

                return listaReclamoRespuestas;
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
