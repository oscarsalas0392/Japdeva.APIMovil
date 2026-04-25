using Microsoft.AspNetCore.Mvc;
using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Parametros.Models;
using Japdeva.APIMovil.Parametros.Services.ParametroCacheService;

namespace Japdeva.APIMovil.Parametros.Services.ObtenerParametroService
{
    /// <summary>
    /// Servicio para obtener parámetros del sistema desde la caché.
    /// </summary>
    public class ObtenerParametroService : IObtenerParametroService
    {
        private readonly ILogger<ObtenerParametroService> _logger;
        private readonly IParametroCacheService _parametroCacheService;

        private const string MENSAJE_PARAMETRO_NO_ENCONTRADO = "No se encontró el parámetro con nombre: ";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerParametroService"/>.
        /// </summary>
        /// <param name="logger">El registrador de eventos para la clase <see cref="ObtenerParametroService"/>.</param>
        /// <param name="parametroCacheService">Servicio de caché de parámetros.</param>
        public ObtenerParametroService(
            ILogger<ObtenerParametroService> logger,
            IParametroCacheService parametroCacheService)
        {
            this._logger = logger;
            this._parametroCacheService = parametroCacheService;
        }

        /// <summary>
        /// Obtiene un parámetro activo por su nombre desde la caché.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="nombre">Nombre del parámetro a obtener.</param>
        /// <returns>Una tarea que representa la operación asincrónica y contiene el resultado de la acción.</returns>
        public Task<IActionResult> ObtenerParametroPorNombreAsync(string traceId, string nombre)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                var parametroEntity = this._parametroCacheService.ObtenerParametroPorNombre(traceId, nombre);

                if (parametroEntity is null)
                    return Task.FromResult<IActionResult>(new NotFoundObjectResult($"{MENSAJE_PARAMETRO_NO_ENCONTRADO}{nombre}"));

                ParametroRespuestaModel respuesta = new ParametroRespuestaModel
                {
                    Id = parametroEntity.Id,
                    Nombre = parametroEntity.Nombre,
                    Valor = parametroEntity.Valor,
                    Descripcion = parametroEntity.Descripcion
                };

                return Task.FromResult<IActionResult>(new OkObjectResult(respuesta));
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
