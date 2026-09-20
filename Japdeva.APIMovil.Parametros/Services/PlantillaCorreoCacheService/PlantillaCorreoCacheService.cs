using Japdeva.APIMovil.Common.Extensions;
using Japdeva.APIMovil.Common.Repositories.ConsultarListaRepository;
using Japdeva.APIMovil.Parametros.Entities;

namespace Japdeva.APIMovil.Parametros.Services.PlantillaCorreoCacheService
{
    /// <summary>
    /// Servicio para gestionar la caché de plantillas de correo en memoria.
    /// </summary>
    public class PlantillaCorreoCacheService : IPlantillaCorreoCacheService
    {
        private readonly ILogger<PlantillaCorreoCacheService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<PlantillaCorreoEntity> _plantillaCorreoCache = new List<PlantillaCorreoEntity>();

        private const int PAGINA_INICIAL = 1;

        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="PlantillaCorreoCacheService"/>.
        /// </summary>
        /// <param name="logger">El registrador de eventos para la clase <see cref="PlantillaCorreoCacheService"/>.</param>
        /// <param name="serviceProvider">El proveedor de servicios para la resolución de dependencias.</param>
        public PlantillaCorreoCacheService(ILogger<PlantillaCorreoCacheService> logger, IServiceProvider serviceProvider)
        {
            this._logger = logger;
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Llena la caché de plantillas de correo obteniendo la información desde el repositorio.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        public async Task LlenarCachePlantillaCorreosAsync(string traceId)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();
            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                List<PlantillaCorreoEntity> listaPlantillaCorreoCache = new List<PlantillaCorreoEntity>();
                var scope = this._serviceProvider.CreateScope();
                var consultarListaEntity = scope.ServiceProvider.GetRequiredService<IConsultarListaRepository>();
                int paginaActual = PAGINA_INICIAL;
                int totalPaginas = PAGINA_INICIAL;

                do
                {
                    var respuestaLista = await consultarListaEntity.ConsultarListaAsync<PlantillaCorreoEntity>(traceId, paginaActual);
                    totalPaginas = respuestaLista.CantidadPaginas;
                    paginaActual++;
                    listaPlantillaCorreoCache.AddRange(respuestaLista.Lista);
                }
                while (paginaActual <= totalPaginas);

                lock (this._plantillaCorreoCache)
                {
                    this._plantillaCorreoCache.Clear();
                    this._plantillaCorreoCache.AddRange(listaPlantillaCorreoCache);
                }
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
        /// Obtiene una plantilla de correo por su identificador.
        /// </summary>
        /// <param name="traceId">Identificador de traza para el seguimiento de la operación.</param>
        /// <param name="id">Identificador de la plantilla de correo.</param>
        /// <returns>La plantilla de correo si existe y está activa, null en caso contrario.</returns>
        public PlantillaCorreoEntity? ObtenerPlantillaCorreoPorId(string traceId, int id)
        {
            string nombreMetodo = this.ObtenerNombreMetodo();

            try
            {
                this._logger.Inicio(traceId, nombreMetodo);
                return this._plantillaCorreoCache.FirstOrDefault(p => p.Id == id && p.Activo);
            }
            catch (Exception ex)
            {
                this._logger.Error(string.Empty, nombreMetodo, ex);
                throw;
            }
            finally
            {
                this._logger.Fin(traceId, nombreMetodo);
            }
        }
    }
}
