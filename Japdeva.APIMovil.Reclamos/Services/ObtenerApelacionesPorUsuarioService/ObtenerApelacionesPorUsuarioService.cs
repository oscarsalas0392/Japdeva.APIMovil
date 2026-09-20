using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.ListaRespuestaApelacionReclamoService;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerApelacionesPorUsuarioService
{
    /// <summary>
    /// Servicio para obtener apelaciones de reclamo filtradas por usuario y estado con soporte de paginación.
    /// </summary>
    public class ObtenerApelacionesPorUsuarioService : IObtenerApelacionesPorUsuarioService
    {
        private readonly ILogger<ObtenerApelacionesPorUsuarioService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IListaRespuestaApelacionReclamoService _listaRespuestaApelacionReclamoService;

        private const string MENSAJE_ERROR_ID_USUARIO_INVALIDO = "El idUsuario es inválido.";
        private const string MENSAJE_ERROR_ID_ESTADO_RECLAMO = "El idEstadoReclamo es inválido.";
        private const string MENSAJE_ERROR_ID_PAGINA = "La pagina es inválida, debe ser mayor a 0.";
        private const int ID_USUARIO_MINIMO = 1;
        private const int ID_ESTADO_MINIMO = 1;
        private const int ID_PAGINA_MINIMO = 1;

        /// <summary>
        /// Inicializa una nueva instancia del servicio de obtención de apelaciones por usuario.
        /// </summary>
        public ObtenerApelacionesPorUsuarioService(
            ILogger<ObtenerApelacionesPorUsuarioService> logger,
            IServiceProvider serviceProvider,
            IListaRespuestaApelacionReclamoService listaRespuestaApelacionReclamoService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._listaRespuestaApelacionReclamoService = listaRespuestaApelacionReclamoService;
        }

        /// <summary>
        /// Obtiene una lista paginada de apelaciones filtradas por usuario y estado.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idUsuario">Identificador del usuario externo propietario de las apelaciones.</param>
        /// <param name="idEstadoReclamo">Identificador del estado a filtrar.</param>
        /// <param name="pagina">Número de página para la consulta paginada.</param>
        /// <returns>Lista paginada de apelaciones del usuario que cumplen los criterios.</returns>
        public async Task<IActionResult> ObtenerApelacionesPorUsuarioAsync(string traceId, int idUsuario, int idEstadoReclamo, int pagina)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                if (idUsuario < ID_USUARIO_MINIMO) throw new ArgumentException(MENSAJE_ERROR_ID_USUARIO_INVALIDO);
                if (idEstadoReclamo < ID_ESTADO_MINIMO) throw new ArgumentException(MENSAJE_ERROR_ID_ESTADO_RECLAMO);
                if (pagina < ID_PAGINA_MINIMO) throw new ArgumentException(MENSAJE_ERROR_ID_PAGINA);

                RespuestaListaModel<ApelacionReclamoRespuestaModel> respuesta = new RespuestaListaModel<ApelacionReclamoRespuestaModel>();

                EstadoReclamoModel estadoReclamo = (EstadoReclamoModel)idEstadoReclamo;

                switch (estadoReclamo)
                {
                    case EstadoReclamoModel.Pendiente:
                        respuesta = await RealizarConsultaApelacionPorUsuarioAsync(traceId, idUsuario, (int)EstadoReclamoModel.Pendiente, pagina);
                        break;

                    case EstadoReclamoModel.EnProceso:
                        respuesta = await RealizarConsultaApelacionPorUsuarioAsync(traceId, idUsuario, (int)EstadoReclamoModel.EnProceso, pagina);
                        break;

                    case EstadoReclamoModel.Completado:
                        respuesta = await RealizarConsultaApelacionPorUsuarioAsync(traceId, idUsuario, (int)EstadoReclamoModel.Completado, pagina);
                        break;

                    case EstadoReclamoModel.Rechazado:
                        respuesta = await RealizarConsultaApelacionPorUsuarioAsync(traceId, idUsuario, (int)EstadoReclamoModel.Rechazado, pagina);
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
        /// Realiza la consulta paginada de apelaciones para un usuario y estado específicos.
        /// </summary>
        /// <param name="traceId">Identificador único para rastreo de la operación.</param>
        /// <param name="idUsuario">Identificador del usuario externo.</param>
        /// <param name="idEstadoReclamo">Identificador del estado a filtrar.</param>
        /// <param name="pagina">Número de página para la consulta paginada.</param>
        /// <returns>Modelo de respuesta con la lista paginada de apelaciones.</returns>
        public async Task<RespuestaListaModel<ApelacionReclamoRespuestaModel>> RealizarConsultaApelacionPorUsuarioAsync(string traceId, int idUsuario, int idEstadoReclamo, int pagina)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                using var scope = this._serviceProvider.CreateScope();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();

                var apelaciones = await consultarListaRepository.ConsultarListaAsync<ApelacionReclamoEntity>(traceId, pagina,
                    x => x.IdUsuarioExterno == idUsuario && x.IdEstadoReclamo == idEstadoReclamo);

                var respuesta = new RespuestaListaModel<ApelacionReclamoRespuestaModel>();
                List<ApelacionReclamoRespuestaModel> lista = await this._listaRespuestaApelacionReclamoService
                    .ObtenerListaRespuestaApelacionReclamoAsync(traceId, apelaciones.Lista);

                respuesta.TotalRegistros = apelaciones.TotalRegistros;
                respuesta.CantidadPaginas = apelaciones.CantidadPaginas;
                respuesta.PaginaActual = apelaciones.PaginaActual;
                respuesta.Lista = lista;
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
