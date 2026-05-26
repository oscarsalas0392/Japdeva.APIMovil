using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Common.Services.ColaRpcService;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerDocumentoInternoPorIdReclamoService
{
    /// <summary>
    /// Servicio para obtener los documentos internos de un reclamo, enriquecidos con la descripción
    /// del detalle de reclamo y el nombre del departamento responsable.
    /// </summary>
    public class ObtenerDocumentoInternoPorIdReclamoService : IObtenerDocumentoInternoPorIdReclamoService
    {
        private readonly ILogger<ObtenerDocumentoInternoPorIdReclamoService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IColaRpcService _colaRpcService;

        private const string MENSAJE_ERROR_PAGINA = "La pagina es inválida, debe ser mayor a 0.";
        private const int PAGINA_MINIMA = 1;
        private const int TIMEOUT_SEGUNDOS = 10;
        private const string COLA_OBTENER_DEPARTAMENTO = "ObtenerDepartamento";
        private const string COLA_OBTENER_USUARIO = "ObtenerUsuario";
        private const string COLA_RESPUESTA = "Respuesta";
        private const long ID_DEPARTAMENTO_DESCONOCIDO = 0;
        private const long ID_USUARIO_DESCONOCIDO = 0;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerDocumentoInternoPorIdReclamoService"/>.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="serviceProvider">Proveedor de servicios para resolución de dependencias.</param>
        /// <param name="colaRpcService">Servicio RPC para comunicación con colas.</param>
        public ObtenerDocumentoInternoPorIdReclamoService(
            ILogger<ObtenerDocumentoInternoPorIdReclamoService> logger,
            IServiceProvider serviceProvider,
            IColaRpcService colaRpcService)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
            this._colaRpcService = colaRpcService;
        }

        /// <summary>
        /// Obtiene los documentos internos de todos los detalles de un reclamo,
        /// incluyendo la descripción del detalle y el nombre del departamento.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idReclamo">Identificador del reclamo a consultar.</param>
        /// <param name="pagina">Número de página para la paginación de resultados.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        public async Task<IActionResult> ObtenerDocumentoInternoPorIdReclamoAsync(string traceId, long idReclamo, int pagina)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                if (pagina < PAGINA_MINIMA) throw new ArgumentException(MENSAJE_ERROR_PAGINA);

                using var scope = this._serviceProvider.CreateScope();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();

                var detalles = await consultarListaRepository.ConsultarListaAsync<DetalleReclamoEntity>(
                    traceId, pagina, x => x.IdReclamo == idReclamo);

                List<long> idsDetalles = detalles.Lista.Select(d => d.Id).ToList();

                var documentos = await consultarListaRepository.ConsultarListaAsync<DocumentoInternoEntity>(
                    traceId, PAGINA_MINIMA, x => idsDetalles.Contains(x.IdDetalleReclamo));

                Dictionary<long, DocumentoInternoEntity> documentoPorDetalle = documentos.Lista
                    .GroupBy(d => d.IdDetalleReclamo)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(d => d.FechaRegistro).First());

                List<long> idsDepartamentosUnicos = detalles.Lista.Select(d => d.IdDepartamento).Distinct().ToList();
                List<long> idsUsuariosUnicos = detalles.Lista.Where(d => d.IdUsuarioInterno is not null && d.IdUsuarioInterno != ID_USUARIO_DESCONOCIDO)
                    .Select(d => d.IdUsuarioInterno!.Value).Distinct().ToList();

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TIMEOUT_SEGUNDOS));

                var tareasDepartamento = idsDepartamentosUnicos.Select(idDep =>
                    this._colaRpcService.EnviarYEsperarRespuestaAsync(traceId, COLA_OBTENER_DEPARTAMENTO, COLA_RESPUESTA, idDep.ToString(), cts.Token)
                        .ContinueWith(t => (id: idDep, mensaje: t.Result), TaskContinuationOptions.ExecuteSynchronously)
                ).ToList();

                var tareasUsuario = idsUsuariosUnicos.Select(idUsu =>
                    this._colaRpcService.EnviarYEsperarRespuestaAsync(traceId, COLA_OBTENER_USUARIO, COLA_RESPUESTA, idUsu.ToString(), cts.Token)
                        .ContinueWith(t => (id: idUsu, mensaje: t.Result), TaskContinuationOptions.ExecuteSynchronously)
                ).ToList();

                await Task.WhenAll(tareasDepartamento.Cast<Task>().Concat(tareasUsuario.Cast<Task>()));

                Dictionary<long, string> nombrePorDepartamento = tareasDepartamento
                    .Select(t => t.Result)
                    .Where(r => r.mensaje is not null)
                    .ToDictionary(
                        r => r.id,
                        r => JsonSerializer.Deserialize<DepartamentoRespuestaModel>(r.mensaje!.Contenido)?.Descripcion ?? string.Empty
                    );

                Dictionary<long, string> nombrePorUsuario = tareasUsuario
                    .Select(t => t.Result)
                    .Where(r => r.mensaje is not null)
                    .ToDictionary(
                        r => r.id,
                        r =>
                        {
                            var datos = JsonSerializer.Deserialize<UsuarioDatosRespuestaModel>(r.mensaje!.Contenido);
                            if (datos is null) return string.Empty;
                            return $"{datos.Nombre} {datos.Apellidos}".Trim();
                        }
                    );

                List<DocumentoInternoRespuestaModel> lista = detalles.Lista.Select(detalle =>
                {
                    documentoPorDetalle.TryGetValue(detalle.Id, out DocumentoInternoEntity? doc);
                    nombrePorDepartamento.TryGetValue(detalle.IdDepartamento, out string? nombreDep);
                    string nombreUsuario = detalle.IdUsuarioInterno is not null
                        && nombrePorUsuario.TryGetValue(detalle.IdUsuarioInterno.Value, out string? nombre)
                        ? nombre : string.Empty;
                    return new DocumentoInternoRespuestaModel
                    {
                        Id = doc?.Id ?? ID_DEPARTAMENTO_DESCONOCIDO,
                        IdDetalleReclamo = detalle.Id,
                        NombreDocumento = doc?.NombreDocumento ?? string.Empty,
                        Documento = doc?.Documento ?? string.Empty,
                        DescripcionDetalleReclamo = detalle.Descripcion,
                        NombreDepartamento = nombreDep ?? string.Empty,
                        FechaInicio = detalle.FechaRegistro,
                        FechaFin = detalle.FechaEdicion,
                        NombreUsuarioInterno = nombreUsuario
                    };
                }).ToList();

                RespuestaListaModel<DocumentoInternoRespuestaModel> respuesta = new RespuestaListaModel<DocumentoInternoRespuestaModel>();
                respuesta.TotalRegistros = detalles.TotalRegistros;
                respuesta.CantidadPaginas = detalles.CantidadPaginas;
                respuesta.PaginaActual = detalles.PaginaActual;
                respuesta.Lista = lista;

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
