using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
using Japdeva.APIMovil.Common.Services.ColaRpcService;
using Japdeva.APIMovil.Reclamos.Models;
using Japdeva.APIMovil.Reclamos.Services.NivelProcesoCacheService;
using Japdeva.APIMovil.Reclamos.Services.OrdenNivelProcesoCacheService;

namespace Japdeva.APIMovil.Reclamos.Services.ObtenerOrdenNivelProcesoService
{
    /// <summary>
    /// Servicio para obtener el orden de los niveles de proceso, utilizando la caché de niveles y órdenes de nivel de proceso.
    /// </summary>
    public class ObtenerOrdenNivelProcesoService : IObtenerOrdenNivelProcesoService
    {
        private readonly ILogger<ObtenerOrdenNivelProcesoService> _logger;
        private readonly IOrdenNivelProcesoCacheService _ordenNivelProcesoCacheService;
        private readonly INivelProcesoCacheService _nivelProcesoCacheService;
        private readonly IColaRpcService _colaRpcService;
        private const int TIMEOUT_SEGUNDOS = 10;
        private const string COLA_OBTENER_DEPARTAMENTO = "ObtenerDepartamento";
        private const string COLA_RESPUESTA = "Respuesta";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerOrdenNivelProcesoService"/>.
        /// </summary>
        /// <param name="logger">El registrador de logs.</param>
        /// <param name="ordenNivelProcesoCacheService">Servicio de caché para las órdenes de nivel de proceso.</param>
        /// <param name="nivelProcesoCacheService">Servicio de caché para los niveles de proceso.</param>
        /// <param name="colaRpcService">Servicio RPC para comunicación con colas con espera de respuesta.</param>
        public ObtenerOrdenNivelProcesoService(
            ILogger<ObtenerOrdenNivelProcesoService> logger,
            IOrdenNivelProcesoCacheService ordenNivelProcesoCacheService,
            INivelProcesoCacheService nivelProcesoCacheService,
            IColaRpcService colaRpcService)
        {
            this._logger = logger;
            this._ordenNivelProcesoCacheService = ordenNivelProcesoCacheService;
            this._nivelProcesoCacheService = nivelProcesoCacheService;
            this._colaRpcService = colaRpcService;
        }

        /// <summary>
        /// Obtiene el orden de los niveles de proceso para un nivel superior y una página específica.
        /// Incluye el nombre del departamento asociado a cada nivel inferior.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idNivelSuperior">Identificador del nivel superior del proceso.</param>
        /// <param name="pagina">Número de página para la paginación de resultados.</param>
        /// <returns>Un resultado de acción que contiene la lista de órdenes de nivel de proceso.</returns>
        public async Task<IActionResult> ObtenerOrdenNivelProcesoAsync(string traceId, int idNivelSuperior, int pagina)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                var ordenesNivelProceso = this._ordenNivelProcesoCacheService.ObtenerLista(traceId, pagina, x => x.IdNivelSuperior == idNivelSuperior);

                List<int> listaNivelesInferiores = ordenesNivelProceso.Lista.Select(x => x.IdNivelInferior).Distinct().ToList();
                var nivelesProceso = this._nivelProcesoCacheService.ObtenerLista(traceId, pagina, x => listaNivelesInferiores.Contains(x.Id));

                List<int> idsUnicos = nivelesProceso.Lista.Select(x => (int)x.IdDepartamento).Distinct().ToList();

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TIMEOUT_SEGUNDOS));

                var tareasDepartamento = idsUnicos.Select(idDep =>
                    this._colaRpcService.EnviarYEsperarRespuestaAsync(traceId, COLA_OBTENER_DEPARTAMENTO, COLA_RESPUESTA, idDep.ToString(), cts.Token)
                        .ContinueWith(t => (idDep, mensaje: t.Result), TaskContinuationOptions.ExecuteSynchronously)
                ).ToList();

                await Task.WhenAll(tareasDepartamento);

                Dictionary<int, string> nombresPorDepartamento = tareasDepartamento
                    .Select(t => t.Result)
                    .Where(r => r.mensaje is not null)
                    .ToDictionary(
                        r => r.idDep,
                        r => JsonSerializer.Deserialize<DepartamentoRespuestaModel>(r.mensaje!.Contenido)?.Descripcion ?? string.Empty
                    );

                RespuestaListaModel<OrdenNivelRespuestaModel> respuesta = new RespuestaListaModel<OrdenNivelRespuestaModel>();
                List<OrdenNivelRespuestaModel> listaRespuesta = new List<OrdenNivelRespuestaModel>();

                foreach (var ordenNivelProceso in ordenesNivelProceso.Lista)
                {
                    var nivelProceso = nivelesProceso.Lista.FirstOrDefault(x => x.Id == ordenNivelProceso.IdNivelInferior);
                    if (nivelProceso is null) continue;

                    int idDep = (int)nivelProceso.IdDepartamento;
                    listaRespuesta.Add(new OrdenNivelRespuestaModel
                    {
                        IdOrdenNivel = ordenNivelProceso.Id,
                        IdNivelSuperior = ordenNivelProceso.IdNivelSuperior,
                        IdNivelInferior = ordenNivelProceso.IdNivelInferior,
                        DevolucionNivel = ordenNivelProceso.DevolucionNivel,
                        IdDepartamento = idDep,
                        DescripcionDepartamento = nombresPorDepartamento.TryGetValue(idDep, out string? desc) ? desc : string.Empty
                    });
                }

                respuesta.Lista = listaRespuesta;
                respuesta.TotalRegistros = ordenesNivelProceso.TotalRegistros;
                respuesta.CantidadPaginas = ordenesNivelProceso.CantidadPaginas;
                respuesta.PaginaActual = ordenesNivelProceso.PaginaActual;

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
