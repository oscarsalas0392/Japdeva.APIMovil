using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;


namespace Japdeva.APIMovil.Reclamos.Services.ObtenerReclamosPorUsuarioService
{
    /// <summary>
    /// Servicio para obtener reclamos filtrados por usuario y estado.
    /// Proporciona funcionalidades para consultar reclamos de un usuario específico
    /// según su estado actual en el sistema con soporte de paginación.
    /// </summary>
    public class ObtenerReclamosPorUsuarioService: IObtenerReclamosPorUsuarioService
    {
        private readonly ILogger<ObtenerReclamosPorUsuarioService> _logger;
        private readonly IConsultarListaRepository _consultarListaRepository;

        private const string MENSAJE_ERROR_ID_USUARIO_INVALIDO = "El idUsuario es inválido.";
        private const string MENSAJE_ERROR_ID_ESTADO_RECLAMO = "El idEstadoReclamo es inválido.";
        private const string MENSAJE_ERROR_ID_PAGINA = "La pagina es inválida, debe ser mayor a 0.";
        private const int ID_USUARIO_MINIMO = 1;
        private const int ID_ESTADO_MINIMO = 1;
        private const int ID_PAGINA_MINIMO = 1;

        /// <summary>
        /// Inicializa una nueva instancia del servicio de obtención de reclamos por usuario.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos y errores.</param>
        /// <param name="consultarListaRepository">Repositorio para consultas paginadas de listas.</param>
        public ObtenerReclamosPorUsuarioService(ILogger<ObtenerReclamosPorUsuarioService> logger,
            IConsultarListaRepository consultarListaRepository)
        {
            this._logger = logger;
            this._consultarListaRepository = consultarListaRepository;
        }

        /// <summary>
        /// Obtiene una lista paginada de reclamos filtrados por usuario y estado.
        /// Consulta los reclamos de un usuario específico según el estado solicitado
        /// y devuelve los resultados con información de paginación.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idUsuario">Identificador del usuario externo propietario de los reclamos.</param>
        /// <param name="idEstadoReclamo">Identificador del estado de reclamo a filtrar.</param>
        /// <param name="pagina">Número de página para la consulta paginada.</param>
        /// <returns>Un IActionResult con la lista paginada de reclamos que cumplen los criterios de filtrado.</returns>
        public async Task<IActionResult> ObtenerReclamosPorUsuarioAsync(string traceId, int idUsuario, int idEstadoReclamo, int pagina)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();

            try
            {
                if(idUsuario < ID_USUARIO_MINIMO)  throw new ArgumentException(MENSAJE_ERROR_ID_USUARIO_INVALIDO);
                if(idEstadoReclamo < ID_ESTADO_MINIMO) throw new ArgumentException(MENSAJE_ERROR_ID_ESTADO_RECLAMO);
                if(pagina < ID_PAGINA_MINIMO) throw new ArgumentException(MENSAJE_ERROR_ID_PAGINA);
                RespuestaListaModel<ReclamoRespuestaModel> respuesta = new RespuestaListaModel<ReclamoRespuestaModel>();
       
                this._logger.Inicio(traceId, nombreMetodo);
                EstadoReclamoModel estadoReclamo = (EstadoReclamoModel)idEstadoReclamo;

                switch (estadoReclamo)
                {

                    case EstadoReclamoModel.Pendiente:
                        respuesta = await RealizarConsultaReclamoPorUsuarioAsync(traceId, idUsuario, (int)EstadoReclamoModel.Pendiente, pagina);
                        break;

                    case EstadoReclamoModel.Completado:
                        respuesta = await RealizarConsultaReclamoPorUsuarioAsync(traceId, idUsuario, (int)EstadoReclamoModel.Completado, pagina);
                        break;

                    case EstadoReclamoModel.EnProceso:
                        respuesta = await RealizarConsultaReclamoPorUsuarioAsync(traceId, idUsuario, (int)EstadoReclamoModel.EnProceso, pagina);
                        break;

                    case EstadoReclamoModel.Rechazado:
                        respuesta = await RealizarConsultaReclamoPorUsuarioAsync(traceId, idUsuario, (int)EstadoReclamoModel.Rechazado, pagina);
                        break;
                }

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

        /// <summary>
        /// Realiza la consulta paginada de reclamos para un usuario específico y un estado de reclamo determinado.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idUsuario">Identificador del usuario externo propietario de los reclamos.</param>
        /// <param name="idEstadoReclamo">Identificador del estado de reclamo a filtrar.</param>
        /// <param name="pagina">Número de página para la consulta paginada.</param>
        /// <returns>Un modelo de respuesta con la lista paginada de reclamos que cumplen los criterios de filtrado.</returns>
        public async Task<RespuestaListaModel<ReclamoRespuestaModel>> RealizarConsultaReclamoPorUsuarioAsync(string traceId, int idUsuario, int idEstadoReclamo, int pagina)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();

            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                var reclamos = await this._consultarListaRepository.ConsultarListaAsync<ReclamoEntity>(traceId, pagina,
                       x => x.IdUsuarioExterno == idUsuario && x.IdEstadoReclamo == idEstadoReclamo);

                var respuesta = new RespuestaListaModel<ReclamoRespuestaModel>();
                respuesta.TotalRegistros = reclamos.TotalRegistros;
                respuesta.CantidadPaginas = reclamos.CantidadPaginas;
                respuesta.PaginaActual = reclamos.PaginaActual;
                respuesta.Lista = reclamos.Lista.Select(r => new ReclamoRespuestaModel()
                {
                    Id = r.Id,
                    Titulo = r.Titulo,
                    Descripcion = r.Descripcion,
                    IdEstadoReclamo = r.IdEstadoReclamo,
                    IdUsuarioExterno = r.IdUsuarioExterno,
                    FechaRegistro = r.FechaRegistro,
                    IdDepartamentoActual = r.IdDepartamentoActual,
                    DescripcionDepartamento = r.DescripcionDepartamentoActual
                }).ToList();

                return respuesta;

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
