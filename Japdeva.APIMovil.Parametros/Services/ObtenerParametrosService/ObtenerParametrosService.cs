using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Parametros.Models;
using Japdeva.APIMovil.Parametros.Services.ParametroCacheService;

namespace Japdeva.APIMovil.Parametros.Services.ObtenerParametrosService
{
    /// <summary>
    /// Servicio para obtener múltiples parámetros del sistema en una sola consulta desde la caché.
    /// </summary>
    public class ObtenerParametrosService : IObtenerParametrosService
    {
        private readonly ILogger<ObtenerParametrosService> _logger;
        private readonly IParametroCacheService _parametroCacheService;

        private const string MENSAJE_LISTA_NULA = "La lista de nombres no puede ser nula o vacía.";
        private const int MINIMO_REGISTROS = 1;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerParametrosService"/>.
        /// </summary>
        /// <param name="logger">El registrador de eventos para la clase.</param>
        /// <param name="parametroCacheService">Servicio de caché de parámetros.</param>
        public ObtenerParametrosService(
            ILogger<ObtenerParametrosService> logger,
            IParametroCacheService parametroCacheService)
        {
            this._logger = logger;
            this._parametroCacheService = parametroCacheService;
        }

        /// <summary>
        /// Obtiene una lista de parámetros activos por sus nombres desde la caché.
        /// Los nombres no encontrados son omitidos del resultado.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="nombres">Lista de nombres de los parámetros a obtener.</param>
        /// <returns>Lista de parámetros encontrados para los nombres indicados.</returns>
        public Task<IActionResult> ObtenerParametrosPorNombresAsync(string traceId, List<string> nombres)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                if (nombres is null || nombres.Count < MINIMO_REGISTROS)
                    throw new ArgumentException(MENSAJE_LISTA_NULA);

                List<ParametroRespuestaModel> resultado = new List<ParametroRespuestaModel>();

                foreach (string nombre in nombres)
                {
                    var parametro = this._parametroCacheService.ObtenerParametroPorNombre(traceId, nombre);
                    if (parametro is null) continue;

                    resultado.Add(new ParametroRespuestaModel
                    {
                        Id = parametro.Id,
                        Nombre = parametro.Nombre,
                        Valor1 = parametro.Valor1,
                        Valor2 = parametro.Valor2,
                        Descripcion = parametro.Descripcion
                    });
                }

                return Task.FromResult<IActionResult>(new OkObjectResult(resultado));
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
