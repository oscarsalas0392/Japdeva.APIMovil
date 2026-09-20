using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Common.Repositories.ConsultarRepository;
using Japdeva.APIMovil.Common.Services.ColaRpcService;
using Japdeva.APIMovil.Reclamos.Entities;
using Japdeva.APIMovil.Reclamos.Models;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerDocumentoInternoPorIdReclamoService
{
    /// <summary>
    /// Servicio para obtener los documentos internos de un reclamo, enriquecidos con la descripción
    /// del detalle de reclamo y el nombre del departamento responsable.
    /// Enruta la consulta a tablas activas o históricas según el valor de <c>EstaEnHistorico</c>.
    /// </summary>
    public class ObtenerDocumentoInternoPorIdReclamoService : IObtenerDocumentoInternoPorIdReclamoService
    {
        private readonly ILogger<ObtenerDocumentoInternoPorIdReclamoService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IColaRpcService _colaRpcService;

        private const string MENSAJE_ERROR_PAGINA = "La pagina es inválida, debe ser mayor a 0.";
        private const string MENSAJE_ERROR_RECLAMO_NO_ENCONTRADO = "El reclamo con Id {0} no fue encontrado.";
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
        /// Obtiene los documentos internos de todos los detalles de un reclamo, incluyendo descripción
        /// del detalle y nombre del departamento. Si el reclamo está en histórico, consulta las tablas
        /// <c>Tbl_DetalleReclamoHistorico</c> y <c>Tbl_DocumentoInternoHistorico</c>.
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
                var consultarRepository = scope.ServiceProvider.GetRequiredService<IConsultarRepository>();
                var consultarListaRepository = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();

                var reclamo = await consultarRepository.ConsultarAsync<ReclamoEntity>(traceId, x => x.Id == idReclamo);
                if (reclamo is null) throw new KeyNotFoundException(string.Format(MENSAJE_ERROR_RECLAMO_NO_ENCONTRADO, idReclamo));

                int totalRegistros;
                int cantidadPaginas;
                int paginaActual;
                List<long> idsDetalles;
                List<long> idsDepartamentosUnicos;
                List<long> idsUsuariosUnicos;
                Dictionary<long, string> descripcionPorDetalle;
                Dictionary<long, DateTime> fechaInicioPorDetalle;
                Dictionary<long, DateTime?> fechaFinPorDetalle;
                Dictionary<long, long> idDepartamentoPorDetalle;
                Dictionary<long, long?> idUsuarioPorDetalle;
                Dictionary<long, (long docId, string nombreDocumento, string documento)> documentoPorDetalle;

                if (reclamo.EstaEnHistorico)
                {
                    var detallesHistorico = await consultarListaRepository.ConsultarListaAsync<DetalleReclamoHistoricoEntity>(
                        traceId, pagina, x => x.IdReclamo == idReclamo);

                    totalRegistros = detallesHistorico.TotalRegistros;
                    cantidadPaginas = detallesHistorico.CantidadPaginas;
                    paginaActual = detallesHistorico.PaginaActual;
                    idsDetalles = detallesHistorico.Lista.Select(d => d.Id).ToList();
                    descripcionPorDetalle = detallesHistorico.Lista.ToDictionary(d => d.Id, d => d.Descripcion);
                    fechaInicioPorDetalle = detallesHistorico.Lista.ToDictionary(d => d.Id, d => d.FechaRegistro);
                    fechaFinPorDetalle = detallesHistorico.Lista.ToDictionary(d => d.Id, d => d.FechaEdicion);
                    idDepartamentoPorDetalle = detallesHistorico.Lista.ToDictionary(d => d.Id, d => d.IdDepartamento);
                    idUsuarioPorDetalle = detallesHistorico.Lista.ToDictionary(d => d.Id, d => d.IdUsuarioInterno);
                    idsDepartamentosUnicos = detallesHistorico.Lista.Select(d => d.IdDepartamento).Distinct().ToList();
                    idsUsuariosUnicos = detallesHistorico.Lista
                        .Where(d => d.IdUsuarioInterno is not null && d.IdUsuarioInterno != ID_USUARIO_DESCONOCIDO)
                        .Select(d => d.IdUsuarioInterno!.Value).Distinct().ToList();

                    var docsHistorico = await consultarListaRepository.ConsultarListaAsync<DocumentoInternoHistoricoEntity>(
                        traceId, PAGINA_MINIMA, x => idsDetalles.Contains(x.IdDetalleReclamo));

                    documentoPorDetalle = docsHistorico.Lista
                        .GroupBy(d => d.IdDetalleReclamo)
                        .ToDictionary(g => g.Key, g =>
                        {
                            var doc = g.OrderByDescending(d => d.FechaRegistro).First();
                            return (doc.Id, doc.NombreDocumento, doc.Documento);
                        });
                }
                else
                {
                    var detallesActivos = await consultarListaRepository.ConsultarListaAsync<DetalleReclamoEntity>(
                        traceId, pagina, x => x.IdReclamo == idReclamo);

                    totalRegistros = detallesActivos.TotalRegistros;
                    cantidadPaginas = detallesActivos.CantidadPaginas;
                    paginaActual = detallesActivos.PaginaActual;
                    idsDetalles = detallesActivos.Lista.Select(d => d.Id).ToList();
                    descripcionPorDetalle = detallesActivos.Lista.ToDictionary(d => d.Id, d => d.Descripcion);
                    fechaInicioPorDetalle = detallesActivos.Lista.ToDictionary(d => d.Id, d => d.FechaRegistro);
                    fechaFinPorDetalle = detallesActivos.Lista.ToDictionary(d => d.Id, d => d.FechaEdicion);
                    idDepartamentoPorDetalle = detallesActivos.Lista.ToDictionary(d => d.Id, d => d.IdDepartamento);
                    idUsuarioPorDetalle = detallesActivos.Lista.ToDictionary(d => d.Id, d => d.IdUsuarioInterno);
                    idsDepartamentosUnicos = detallesActivos.Lista.Select(d => d.IdDepartamento).Distinct().ToList();
                    idsUsuariosUnicos = detallesActivos.Lista
                        .Where(d => d.IdUsuarioInterno is not null && d.IdUsuarioInterno != ID_USUARIO_DESCONOCIDO)
                        .Select(d => d.IdUsuarioInterno!.Value).Distinct().ToList();

                    var docsActivos = await consultarListaRepository.ConsultarListaAsync<DocumentoInternoEntity>(
                        traceId, PAGINA_MINIMA, x => idsDetalles.Contains(x.IdDetalleReclamo));

                    documentoPorDetalle = docsActivos.Lista
                        .GroupBy(d => d.IdDetalleReclamo)
                        .ToDictionary(g => g.Key, g =>
                        {
                            var doc = g.OrderByDescending(d => d.FechaRegistro).First();
                            return (doc.Id, doc.NombreDocumento, doc.Documento);
                        });
                }

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

                List<DocumentoInternoRespuestaModel> lista = idsDetalles.Select(detalleId =>
                {
                    bool tieneDoc = documentoPorDetalle.TryGetValue(detalleId, out var doc);
                    idDepartamentoPorDetalle.TryGetValue(detalleId, out long idDept);
                    nombrePorDepartamento.TryGetValue(idDept, out string? nombreDep);

                    idUsuarioPorDetalle.TryGetValue(detalleId, out long? idUsuario);
                    string nombreUsuario = idUsuario is not null
                        && nombrePorUsuario.TryGetValue(idUsuario.Value, out string? nombre)
                        ? nombre : string.Empty;

                    descripcionPorDetalle.TryGetValue(detalleId, out string? descripcion);
                    fechaInicioPorDetalle.TryGetValue(detalleId, out DateTime fechaInicio);
                    fechaFinPorDetalle.TryGetValue(detalleId, out DateTime? fechaFin);

                    return new DocumentoInternoRespuestaModel
                    {
                        Id = tieneDoc ? doc.docId : ID_DEPARTAMENTO_DESCONOCIDO,
                        IdDetalleReclamo = detalleId,
                        NombreDocumento = tieneDoc ? doc.nombreDocumento : string.Empty,
                        Documento = tieneDoc ? doc.documento : string.Empty,
                        DescripcionDetalleReclamo = descripcion ?? string.Empty,
                        NombreDepartamento = nombreDep ?? string.Empty,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin,
                        NombreUsuarioInterno = nombreUsuario
                    };
                }).ToList();

                RespuestaListaModel<DocumentoInternoRespuestaModel> respuesta = new RespuestaListaModel<DocumentoInternoRespuestaModel>();
                respuesta.TotalRegistros = totalRegistros;
                respuesta.CantidadPaginas = cantidadPaginas;
                respuesta.PaginaActual = paginaActual;
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
