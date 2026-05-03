using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EstadoReclamoCacheService;

namespace Japdeva.APIMovil.Reclamos.Services.ListaRespuestaApelacionReclamoService
{
    /// <summary>
    /// Servicio para convertir entidades de apelación de reclamo en modelos de respuesta.
    /// </summary>
    public class ListaRespuestaApelacionReclamoService : IListaRespuestaApelacionReclamoService
    {
        private readonly ILogger<ListaRespuestaApelacionReclamoService> _logger;
        private readonly IEstadoReclamoCacheService _estadoReclamoCacheService;

        private const string MENSAJE_ERROR_ESTADO_RECLAMO_NO_EXISTE = "No se encontró el estado de reclamo en caché para el Id:{0}";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ListaRespuestaApelacionReclamoService"/>.
        /// </summary>
        public ListaRespuestaApelacionReclamoService(
            ILogger<ListaRespuestaApelacionReclamoService> logger,
            IEstadoReclamoCacheService estadoReclamoCacheService)
        {
            this._logger = logger;
            this._estadoReclamoCacheService = estadoReclamoCacheService;
        }

        /// <summary>
        /// Convierte una lista de entidades de apelación en modelos de respuesta enriquecidos con datos del estado.
        /// </summary>
        public async Task<List<ApelacionReclamoRespuestaModel>> ObtenerListaRespuestaApelacionReclamoAsync(string traceId, List<ApelacionReclamoEntity> listaApelacionEntity)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                List<ApelacionReclamoRespuestaModel> lista = new List<ApelacionReclamoRespuestaModel>();

                foreach (var apelacion in listaApelacionEntity)
                {
                    var estadoCache = this._estadoReclamoCacheService.ObtenerEstadoReclamo(traceId, apelacion.IdEstadoReclamo);
                    if (estadoCache is null) throw new Exception(string.Format(MENSAJE_ERROR_ESTADO_RECLAMO_NO_EXISTE, apelacion.IdEstadoReclamo));

                    ApelacionReclamoRespuestaModel modelo = new ApelacionReclamoRespuestaModel();
                    modelo.Id = apelacion.Id;
                    modelo.IdReclamo = apelacion.IdReclamo;
                    modelo.Titulo = apelacion.Titulo;
                    modelo.Descripcion = apelacion.Descripcion;
                    modelo.IdEstadoReclamo = apelacion.IdEstadoReclamo;
                    modelo.DescripcionEstadoReclamo = estadoCache.Descripcion;
                    modelo.IdUsuarioExterno = apelacion.IdUsuarioExterno;
                    modelo.FechaRegistro = apelacion.FechaRegistro;
                    modelo.IdDepartamentoActual = apelacion.IdDepartamentoActual;
                    modelo.DescripcionDepartamento = string.Empty;
                    lista.Add(modelo);
                }

                return lista;
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
