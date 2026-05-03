using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Models;
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

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerOrdenNivelProcesoService"/>.
        /// </summary>
        /// <param name="logger">El registrador de logs.</param>
        /// <param name="ordenNivelProcesoCacheService">Servicio de caché para las órdenes de nivel de proceso.</param>
        /// <param name="nivelProcesoCacheService">Servicio de caché para los niveles de proceso.</param>
        public ObtenerOrdenNivelProcesoService(
            ILogger<ObtenerOrdenNivelProcesoService> logger,
            IOrdenNivelProcesoCacheService ordenNivelProcesoCacheService,
            INivelProcesoCacheService nivelProcesoCacheService)
        {
            this._logger = logger;
            this._ordenNivelProcesoCacheService = ordenNivelProcesoCacheService;
            this._nivelProcesoCacheService = nivelProcesoCacheService;
        }

        /// <summary>
        /// Obtiene el orden de los niveles de proceso para un nivel superior y una página específica.
        /// Utiliza la caché de niveles y órdenes de nivel de proceso para construir la respuesta.
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
                RespuestaListaModel<OrdenNivelRespuestaModel> respuesta = new RespuestaListaModel<OrdenNivelRespuestaModel>();
                List<OrdenNivelRespuestaModel> listaRespuesta = new List<OrdenNivelRespuestaModel>();

                List<int> listaNiveles = ordenesNivelProceso.Lista.Select(x => x.IdNivelInferior).Distinct().ToList();
                
                var nivelesProceso = this._nivelProcesoCacheService.ObtenerLista( traceId, pagina, x => listaNiveles.Contains(x.Id));

                List<int> listaIdDepartamentos = nivelesProceso.Lista.Select(x => (int)x.IdDepartamento).Distinct().ToList();

                // LLAMAR MICROSERVICIO DE USUARIOS

                foreach (var ordenNivelProceso in ordenesNivelProceso.Lista)
                {
                    OrdenNivelRespuestaModel ordenNivelProcesoRespuestaModel = new OrdenNivelRespuestaModel();
                    var nivelProceso = nivelesProceso.Lista.FirstOrDefault(x=>x.Id == ordenNivelProceso.IdNivelInferior);
                    if(nivelProceso is null) continue;
                    ordenNivelProcesoRespuestaModel.IdOrdenNivel = ordenNivelProceso.Id;
                    ordenNivelProcesoRespuestaModel.IdNivelSuperior = ordenNivelProceso.IdNivelSuperior;
                    ordenNivelProcesoRespuestaModel.IdNivelInferior = ordenNivelProceso.IdNivelInferior;
                    ordenNivelProcesoRespuestaModel.DevolucionNivel = ordenNivelProceso.DevolucionNivel;
                    ordenNivelProcesoRespuestaModel.IdDepartamento = nivelProceso.IdDepartamento;
                    ordenNivelProcesoRespuestaModel.DescripcionDepartamento = "";
                    listaRespuesta.Add(ordenNivelProcesoRespuestaModel);
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
