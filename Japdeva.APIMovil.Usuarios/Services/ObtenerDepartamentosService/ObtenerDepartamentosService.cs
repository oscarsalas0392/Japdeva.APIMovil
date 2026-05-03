using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Usuarios.Models;
using Japdeva.APIMovil.Usuarios.Services.DepartamentoCacheService;

namespace Japdeva.APIMovil.Usuarios.Services.ObtenerDepartamentosService
{
    /// <summary>
    /// Servicio para consultar los departamentos desde caché.
    /// </summary>
    public class ObtenerDepartamentosService : IObtenerDepartamentosService
    {
        private readonly ILogger<ObtenerDepartamentosService> _logger;
        private readonly IDepartamentoCacheService _departamentoCacheService;
        private const string MENSAJE_DEPARTAMENTO_NO_ENCONTRADO = "Departamento no encontrado.";

        /// <summary>
        /// Inicializa una nueva instancia de ObtenerDepartamentosService.
        /// </summary>
        /// <param name="logger">Logger para registro de eventos.</param>
        /// <param name="departamentoCacheService">Servicio de caché de departamentos.</param>
        public ObtenerDepartamentosService(ILogger<ObtenerDepartamentosService> logger, IDepartamentoCacheService departamentoCacheService)
        {
            this._logger = logger;
            this._departamentoCacheService = departamentoCacheService;
        }

        /// <summary>
        /// Retorna todos los departamentos activos almacenados en caché.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <returns>Lista de departamentos activos.</returns>
        public async Task<IActionResult> ObtenerTodosLosDepartamentosAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                var departamentos = this._departamentoCacheService.ObtenerTodos(traceId);
                var lista = new List<DepartamentoRespuestaModel>();
                foreach (var dep in departamentos)
                {
                    var modelo = new DepartamentoRespuestaModel();
                    modelo.Id = dep.Id;
                    modelo.Descripcion = dep.Descripcion;
                    modelo.Activo = dep.Activo;
                    lista.Add(modelo);
                }

                return new OkObjectResult(lista);
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
        /// Retorna el departamento activo con el identificador indicado.
        /// </summary>
        /// <param name="traceId">Identificador de trazabilidad.</param>
        /// <param name="id">Identificador del departamento.</param>
        /// <returns>Resultado con el departamento encontrado.</returns>
        public async Task<IActionResult> ObtenerDepartamentoPorIdAsync(string traceId, int id)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                var departamento = this._departamentoCacheService.ObtenerPorId(traceId, id);
                if (departamento is null) throw new KeyNotFoundException(MENSAJE_DEPARTAMENTO_NO_ENCONTRADO);
             
                var respuesta = new DepartamentoRespuestaModel();
                respuesta.Id = departamento.Id;
                respuesta.Descripcion = departamento.Descripcion;
                respuesta.Activo = departamento.Activo;
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
