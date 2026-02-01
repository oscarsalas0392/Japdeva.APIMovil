using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Parametros.Models;
using Japdeva.APIMovil.Parametros.Services.PlantillaCorreoCacheService;

namespace Japdeva.APIMovil.Parametros.Services.ObtenerPlantillaNuevoReclamoService
{
    /// <summary>
    /// Servicio para obtener la plantilla de correo de nuevo reclamo con los datos personalizados.
    /// </summary>
    public class ObtenerPlantillaNuevoReclamoService : IObtenerPlantillaNuevoReclamoService
    {
        private readonly ILogger<ObtenerPlantillaNuevoReclamoService> _logger;
        private readonly IPlantillaCorreoCacheService _plantillaCorreoCacheService;

        private const string MARCADOR_ID_RECLAMO = "@idReclamo";
        private const string MENSAJE_ERROR_PLANTILLA_NO_ENCONTRADA = "No se encontró la plantilla de correo con ID ";

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ObtenerPlantillaNuevoReclamoService"/>.
        /// </summary>
        /// <param name="logger">El registrador de eventos para la clase <see cref="ObtenerPlantillaNuevoReclamoService"/>.</param>
        /// <param name="plantillaCorreoCacheService">Servicio de caché de plantillas de correo.</param>
        public ObtenerPlantillaNuevoReclamoService(
            ILogger<ObtenerPlantillaNuevoReclamoService> logger,
            IPlantillaCorreoCacheService plantillaCorreoCacheService)
        {
            this._logger = logger;
            this._plantillaCorreoCacheService = plantillaCorreoCacheService;
        }

        /// <summary>
        /// Obtiene la plantilla de correo para nuevo reclamo, reemplazando el marcador @idReclamo por el valor proporcionado.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="idReclamo">Identificador del reclamo que se usará para personalizar la plantilla.</param>
        /// <returns>La plantilla de correo personalizada con el ID del reclamo.</returns>
        public string ObtenerPlantillaNuevoReclamo(string traceId, long idReclamo)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);

                var plantillaEntity = this._plantillaCorreoCacheService.ObtenerPlantillaCorreoPorId(
                    traceId,
                    (int)TipoPlantillaCorreoModel.NuevoReclamo);

                if (plantillaEntity is null)
                {
                    throw new Exception($"{MENSAJE_ERROR_PLANTILLA_NO_ENCONTRADA}{(int)TipoPlantillaCorreoModel.NuevoReclamo}");
                }

                string plantillaPersonalizada = plantillaEntity.Plantilla.Replace(MARCADOR_ID_RECLAMO, idReclamo.ToString());

                return plantillaPersonalizada;
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
