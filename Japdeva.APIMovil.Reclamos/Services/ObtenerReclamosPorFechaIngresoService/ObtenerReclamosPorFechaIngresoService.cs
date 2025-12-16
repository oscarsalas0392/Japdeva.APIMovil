using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.EstadoReclamoCacheService;
using Japdeva.APIMovil.Reclamos.Services.ListaRespuestaReclamoService;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerReclamosPorFechaIngresoService
{
    /// <summary>
    /// Servicio para obtener la lista de reclamos filtrados por fecha de ingreso y página.
    /// </summary>
    public class ObtenerReclamosPorFechaIngresoService: IObtenerReclamosPorFechaIngresoService
    {
        private readonly ILogger<ObtenerReclamosPorFechaIngresoService> _logger;
        private readonly IConsultarListaRepository _consultarListaRepository;
        private readonly IEstadoReclamoCacheService _estadoReclamoCacheService;
        private readonly IListaRespuestaReclamoService _listaRespuestaReclamoService;

        private const string MENSAJE_ERROR_ID_PAGINA = "La pagina es inválida, debe ser mayor a 0.";
        private const string MENSAJE_ERROR_ESTADO_RECLAMO_NO_EXISTE = "No se encontró el estado de reclamo en caché para el Id:{0}";
        private const int ID_PAGINA_MINIMO = 1;


        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerReclamosPorFechaIngresoService"/>.
        /// </summary>
        /// <param name="logger">Instancia del logger para el registro de logs.</param>
        /// <param name="consultarListaRepository">Repositorio para la consulta de listas de reclamos.</param>
        public ObtenerReclamosPorFechaIngresoService(
            ILogger<ObtenerReclamosPorFechaIngresoService> logger,
            IConsultarListaRepository consultarListaRepository,
            IEstadoReclamoCacheService estadoReclamoCacheService,
            IListaRespuestaReclamoService listaRespuestaReclamoService)
        {
            this._logger = logger;
            this._consultarListaRepository = consultarListaRepository;
            this._estadoReclamoCacheService = estadoReclamoCacheService;
            this._listaRespuestaReclamoService = listaRespuestaReclamoService;
        }

        /// <summary>
        /// Obtiene la lista de reclamos filtrados por fecha de ingreso y página.
        /// </summary>
        /// <param name="traceId">Identificador de la traza para el registro de logs.</param>
        /// <param name="fechaInicio">Fecha de inicio para filtrar los reclamos.</param>
        /// <param name="fechaFin">Fecha de fin para filtrar los reclamos.</param>
        /// <param name="estadoReclamo">Estado del reclamo para filtrar.</param>
        /// <param name="pagina">Número de página para la paginación de resultados.</param>
        /// <returns>Una acción que contiene la lista de reclamos filtrados.</returns>
        public async Task<IActionResult> ObtenerReclamosPorFechaIngresoAsync(string traceId, DateTime fechaInicio, DateTime fechaFin, int estadoReclamo, int pagina) 
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (pagina < ID_PAGINA_MINIMO) throw new ArgumentException(MENSAJE_ERROR_ID_PAGINA);
                var respuesta = new RespuestaListaModel<ReclamoRespuestaModel>();
                List<ReclamoRespuestaModel> listaReclamoRespuestas = new List<ReclamoRespuestaModel>();

                var reclamos = await this._consultarListaRepository.ConsultarListaAsync<ReclamoEntity>(traceId, pagina,
                     x => x.FechaRegistro >= fechaInicio && x.FechaRegistro <= fechaFin && x.IdEstadoReclamo == estadoReclamo);


                List<long> listaIdDepartamento = reclamos.Lista
                                   .Select(x => x.IdDepartamentoActual)
                                   .Distinct()
                                   .ToList();

                //AQUI FATAL CONSULTAR MICROSERVICIO DE USUARIOS

                foreach(var reclamoEntity in reclamos.Lista)
                {
                    var estadoReclamoCache = this._estadoReclamoCacheService.ObtenerEstadoReclamo(traceId, reclamoEntity.IdEstadoReclamo);
                    if(estadoReclamoCache is null) throw new Exception(string.Format(MENSAJE_ERROR_ESTADO_RECLAMO_NO_EXISTE, reclamoEntity.IdEstadoReclamo));
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
               
                respuesta.TotalRegistros = reclamos.TotalRegistros;
                respuesta.CantidadPaginas = reclamos.CantidadPaginas;
                respuesta.PaginaActual = reclamos.PaginaActual;
                respuesta.Lista = listaReclamoRespuestas;

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
